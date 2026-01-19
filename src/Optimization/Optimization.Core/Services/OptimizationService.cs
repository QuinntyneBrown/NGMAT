using System.Diagnostics;
using Optimization.Core.Entities;
using Optimization.Core.Events;
using Optimization.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Optimization.Core.Services;

public sealed class OptimizationService
{
    private readonly IOptimizationUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly Dictionary<Guid, CancellationTokenSource> _runningJobs = new();

    public OptimizationService(IOptimizationUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<OptimizationJob>> CreateJobAsync(
        string name,
        OptimizationAlgorithm algorithm,
        OptimizationObjective objective,
        string createdByUserId,
        List<(string name, double initial, double lower, double upper)> designVariables,
        int maxIterations = 1000,
        double convergenceTolerance = 1e-8,
        string? description = null,
        Guid? missionId = null,
        CancellationToken cancellationToken = default)
    {
        var job = OptimizationJob.Create(
            name, algorithm, objective, createdByUserId,
            maxIterations, convergenceTolerance, description, missionId);

        foreach (var dv in designVariables)
        {
            job.AddDesignVariable(dv.name, dv.initial, dv.lower, dv.upper);
        }

        await _unitOfWork.Jobs.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OptimizationJob>.Success(job);
    }

    public async Task<Result<OptimizationJob>> GetJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(id, cancellationToken);
        if (job == null || job.IsDeleted)
        {
            return Result<OptimizationJob>.Failure(Error.NotFound("OptimizationJob", id.ToString()));
        }
        return Result<OptimizationJob>.Success(job);
    }

    public async Task<Result<IReadOnlyList<OptimizationJob>>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _unitOfWork.Jobs.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<OptimizationJob>>.Success(jobs);
    }

    public async Task<Result<IReadOnlyList<OptimizationJob>>> GetJobsByStatusAsync(
        OptimizationStatus status,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _unitOfWork.Jobs.GetByStatusAsync(status, cancellationToken);
        return Result<IReadOnlyList<OptimizationJob>>.Success(jobs);
    }

    public async Task<Result> DeleteJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            return Result.Failure(Error.NotFound("OptimizationJob", id.ToString()));
        }

        job.Delete();
        await _unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<OptimizationJob>> RunOptimizationAsync(
        Guid jobId,
        ObjectiveFunction objectiveFunction,
        CancellationToken cancellationToken = default)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId, cancellationToken);
        if (job == null || job.IsDeleted)
        {
            return Result<OptimizationJob>.Failure(Error.NotFound("OptimizationJob", jobId.ToString()));
        }

        if (job.Status != OptimizationStatus.Queued)
        {
            return Result<OptimizationJob>.Failure(Error.Validation($"Job is in {job.Status} status and cannot be started"));
        }

        // Create cancellation token for this job
        var jobCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runningJobs[jobId] = jobCts;

        try
        {
            job.Start();
            await _unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new OptimizationStartedEvent
            {
                JobId = job.Id,
                Name = job.Name,
                Algorithm = job.Algorithm.ToString(),
                Objective = job.Objective.ToString(),
                MaxIterations = job.MaxIterations
            }, cancellationToken);

            // Get optimizer
            var optimizer = OptimizerFactory.Create(job.Algorithm);

            // Prepare initial guess and bounds
            var initialGuess = job.DesignVariables.Select(dv => dv.InitialValue).ToArray();
            var lowerBounds = job.DesignVariables.Select(dv => dv.LowerBound).ToArray();
            var upperBounds = job.DesignVariables.Select(dv => dv.UpperBound).ToArray();

            // Run optimization
            var result = optimizer.Optimize(
                objectiveFunction,
                initialGuess,
                lowerBounds,
                upperBounds,
                job.MaxIterations,
                job.ConvergenceTolerance,
                (iter, cost, solution) =>
                {
                    job.UpdateProgress(iter, cost, solution);

                    // Publish progress every 10 iterations
                    if (iter % 10 == 0)
                    {
                        _ = _eventPublisher.PublishAsync(new OptimizationProgressEvent
                        {
                            JobId = job.Id,
                            CurrentIteration = iter,
                            CurrentCost = cost,
                            BestCost = job.BestCost
                        }, CancellationToken.None);
                    }
                },
                jobCts.Token);

            job.Complete(result.Solution, result.ObjectiveValue, result.Converged);
            await _unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new OptimizationCompletedEvent
            {
                JobId = job.Id,
                Name = job.Name,
                Converged = result.Converged,
                Iterations = result.Iterations,
                FinalCost = result.ObjectiveValue,
                ComputationTimeMs = job.ComputationTimeMs,
                TerminationReason = result.TerminationReason
            }, cancellationToken);

            return Result<OptimizationJob>.Success(job);
        }
        catch (OperationCanceledException)
        {
            job.Cancel();
            await _unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new OptimizationCancelledEvent
            {
                JobId = job.Id
            }, CancellationToken.None);

            return Result<OptimizationJob>.Failure(Error.Validation("Optimization was cancelled"));
        }
        catch (Exception ex)
        {
            job.Fail(ex.Message);
            await _unitOfWork.Jobs.UpdateAsync(job, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            await _eventPublisher.PublishAsync(new OptimizationFailedEvent
            {
                JobId = job.Id,
                Name = job.Name,
                ErrorMessage = ex.Message
            }, CancellationToken.None);

            return Result<OptimizationJob>.Failure(Error.Internal($"Optimization failed: {ex.Message}", ex));
        }
        finally
        {
            _runningJobs.Remove(jobId);
            jobCts.Dispose();
        }
    }

    public async Task<Result> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _unitOfWork.Jobs.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
        {
            return Result.Failure(Error.NotFound("OptimizationJob", jobId.ToString()));
        }

        if (_runningJobs.TryGetValue(jobId, out var cts))
        {
            cts.Cancel();
        }
        else
        {
            // Job not running, just mark as cancelled
            try
            {
                job.Cancel();
                await _unitOfWork.Jobs.UpdateAsync(job, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Validation(ex.Message));
            }
        }

        return Result.Success();
    }

    // Utility optimization methods for common objective functions

    public Result<OptimizationResult> MinimizeFunction(
        ObjectiveFunction objective,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        OptimizationAlgorithm algorithm = OptimizationAlgorithm.NelderMead,
        int maxIterations = 1000,
        double tolerance = 1e-8,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var optimizer = OptimizerFactory.Create(algorithm);
            var result = optimizer.Optimize(
                objective,
                initialGuess,
                lowerBounds,
                upperBounds,
                maxIterations,
                tolerance,
                cancellationToken: cancellationToken);

            return Result<OptimizationResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<OptimizationResult>.Failure(Error.Internal($"Optimization failed: {ex.Message}", ex));
        }
    }

    public Result<double[]> FindRoots(
        Func<double[], double[]> equations,
        double[] initialGuess,
        double[] lowerBounds,
        double[] upperBounds,
        int maxIterations = 100,
        double tolerance = 1e-10,
        CancellationToken cancellationToken = default)
    {
        // Convert root-finding to minimization problem
        ObjectiveFunction sumOfSquares = x =>
        {
            var residuals = equations(x);
            return residuals.Sum(r => r * r);
        };

        var result = MinimizeFunction(
            sumOfSquares,
            initialGuess,
            lowerBounds,
            upperBounds,
            OptimizationAlgorithm.NelderMead,
            maxIterations,
            tolerance * tolerance,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result<double[]>.Failure(result.Error);
        }

        if (result.Value!.ObjectiveValue > tolerance)
        {
            return Result<double[]>.Failure(Error.Validation($"Root finding did not converge. Residual: {result.Value.ObjectiveValue}"));
        }

        return Result<double[]>.Success(result.Value.Solution);
    }
}
