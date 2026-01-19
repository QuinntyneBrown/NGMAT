using System.Diagnostics;
using Propagation.Core.Entities;
using Propagation.Core.Events;
using Propagation.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Propagation.Core.Services;

public delegate StateDerivative AccelerationProvider(DateTime epoch, PropagationState state);

public sealed class PropagationService
{
    private readonly IPropagationUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public PropagationService(IPropagationUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<PropagationConfiguration>> CreateConfigurationAsync(
        string name,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Configurations.ExistsByNameAsync(name, cancellationToken))
        {
            return Result<PropagationConfiguration>.Failure(Error.Conflict($"A propagation configuration with name '{name}' already exists"));
        }

        var config = PropagationConfiguration.Create(name, createdByUserId, description, missionId);

        await _unitOfWork.Configurations.AddAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new PropagationConfigurationCreatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            IntegratorType = config.Integrator.ToString()
        }, cancellationToken);

        return Result<PropagationConfiguration>.Success(config);
    }

    public async Task<Result<PropagationConfiguration>> GetConfigurationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<PropagationConfiguration>.Failure(Error.NotFound("PropagationConfiguration", id.ToString()));
        }
        return Result<PropagationConfiguration>.Success(config);
    }

    public async Task<Result<IReadOnlyList<PropagationConfiguration>>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        var configs = await _unitOfWork.Configurations.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<PropagationConfiguration>>.Success(configs);
    }

    public async Task<Result<PropagationConfiguration>> UpdateIntegratorSettingsAsync(
        Guid id,
        IntegratorType integrator,
        double initialStepSize,
        double minStepSize,
        double maxStepSize,
        double relativeTolerance,
        double absoluteTolerance,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<PropagationConfiguration>.Failure(Error.NotFound("PropagationConfiguration", id.ToString()));
        }

        config.SetIntegratorSettings(integrator, initialStepSize, minStepSize, maxStepSize, relativeTolerance, absoluteTolerance);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new PropagationConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "IntegratorSettings"
        }, cancellationToken);

        return Result<PropagationConfiguration>.Success(config);
    }

    public async Task<Result<PropagationConfiguration>> UpdateOutputSettingsAsync(
        Guid id,
        OutputMode outputMode,
        double outputStepSizeSeconds,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null || config.IsDeleted)
        {
            return Result<PropagationConfiguration>.Failure(Error.NotFound("PropagationConfiguration", id.ToString()));
        }

        config.SetOutputSettings(outputMode, outputStepSizeSeconds);
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new PropagationConfigurationUpdatedEvent
        {
            ConfigurationId = config.Id,
            Name = config.Name,
            UpdatedField = "OutputSettings"
        }, cancellationToken);

        return Result<PropagationConfiguration>.Success(config);
    }

    public async Task<Result> DeleteConfigurationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var config = await _unitOfWork.Configurations.GetByIdAsync(id, cancellationToken);
        if (config == null)
        {
            return Result.Failure(Error.NotFound("PropagationConfiguration", id.ToString()));
        }

        config.Delete();
        await _unitOfWork.Configurations.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new PropagationConfigurationDeletedEvent
        {
            ConfigurationId = config.Id
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> CreateStandardConfigurationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("Fast Propagation", cancellationToken))
        {
            var fast = StandardPropagationConfigs.CreateFastPropagation(userId);
            await _unitOfWork.Configurations.AddAsync(fast, cancellationToken);
            count++;
        }

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("Precise Propagation", cancellationToken))
        {
            var precise = StandardPropagationConfigs.CreatePrecisePropagation(userId);
            await _unitOfWork.Configurations.AddAsync(precise, cancellationToken);
            count++;
        }

        if (!await _unitOfWork.Configurations.ExistsByNameAsync("Long Term Propagation", cancellationToken))
        {
            var longTerm = StandardPropagationConfigs.CreateLongTermPropagation(userId);
            await _unitOfWork.Configurations.AddAsync(longTerm, cancellationToken);
            count++;
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(count);
    }

    public async Task<Result<PropagationResult>> PropagateAsync(
        PropagationState initialState,
        DateTime endEpoch,
        AccelerationProvider accelerationProvider,
        PropagationConfiguration? config = null,
        Guid? spacecraftId = null,
        CancellationToken cancellationToken = default)
    {
        config ??= StandardPropagationConfigs.CreatePrecisePropagation("system");

        var propagationId = Guid.NewGuid();
        await _eventPublisher.PublishAsync(new PropagationStartedEvent
        {
            PropagationId = propagationId,
            SpacecraftId = spacecraftId,
            StartEpoch = initialState.Epoch,
            EndEpoch = endEpoch
        }, cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = PropagateInternal(initialState, endEpoch, accelerationProvider, config, cancellationToken);
            stopwatch.Stop();

            var propagationResult = PropagationResult.CreateSuccessful(
                initialState.Epoch,
                result.states.LastOrDefault().Epoch,
                result.states,
                result.stepCount,
                stopwatch.ElapsedMilliseconds,
                result.terminationReason,
                config.Id,
                spacecraftId);

            await _unitOfWork.Results.AddAsync(propagationResult, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new PropagationCompletedEvent
            {
                PropagationId = propagationResult.Id,
                SpacecraftId = spacecraftId,
                StartEpoch = propagationResult.StartEpoch,
                EndEpoch = propagationResult.EndEpoch,
                StateCount = propagationResult.States.Count,
                StepCount = propagationResult.StepCount,
                ComputationTimeMs = propagationResult.ComputationTimeMs,
                WasSuccessful = true,
                TerminationReason = result.terminationReason.ToString()
            }, cancellationToken);

            return Result<PropagationResult>.Success(propagationResult);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var failedResult = PropagationResult.CreateFailed(
                initialState.Epoch,
                ex.Message,
                PropagationTerminationReason.IntegrationError,
                config.Id,
                spacecraftId);

            await _eventPublisher.PublishAsync(new PropagationFailedEvent
            {
                PropagationId = failedResult.Id,
                SpacecraftId = spacecraftId,
                ErrorMessage = ex.Message,
                TerminationReason = PropagationTerminationReason.IntegrationError.ToString()
            }, cancellationToken);

            return Result<PropagationResult>.Failure(Error.Internal($"Propagation failed: {ex.Message}", ex));
        }
    }

    private (List<PropagationState> states, int stepCount, PropagationTerminationReason terminationReason) PropagateInternal(
        PropagationState initialState,
        DateTime endEpoch,
        AccelerationProvider accelerationProvider,
        PropagationConfiguration config,
        CancellationToken cancellationToken)
    {
        var states = new List<PropagationState> { initialState };
        var currentState = initialState;
        var stepSize = config.InitialStepSizeSeconds;
        var stepCount = 0;
        var direction = endEpoch > initialState.Epoch ? 1.0 : -1.0;
        var nextOutputTime = initialState.Epoch.AddSeconds(config.OutputStepSizeSeconds * direction);

        var integrator = IntegratorFactory.Create(config.Integrator);

        // Create derivative function that wraps the acceleration provider
        StateDerivative derivativeFunc(DateTime epoch, PropagationState state)
        {
            var accel = accelerationProvider(epoch, state);
            return new StateDerivative(state.Vx, state.Vy, state.Vz, accel.Ax, accel.Ay, accel.Az);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Check stopping conditions
            if (config.MaxStepCount.HasValue && stepCount >= config.MaxStepCount.Value)
            {
                return (states, stepCount, PropagationTerminationReason.ReachedMaxSteps);
            }

            if (config.MinAltitudeMeters.HasValue && currentState.Altitude < config.MinAltitudeMeters.Value)
            {
                return (states, stepCount, PropagationTerminationReason.BelowMinAltitude);
            }

            // Determine step size to not overshoot end epoch
            var remainingTime = (endEpoch - currentState.Epoch).TotalSeconds;
            var actualStepSize = Math.Min(Math.Abs(stepSize), Math.Abs(remainingTime)) * direction;

            if (Math.Abs(remainingTime) < 1e-6)
            {
                return (states, stepCount, PropagationTerminationReason.ReachedEndEpoch);
            }

            // Take integration step
            var (newState, stepTaken, errorEstimate) = integrator.Step(currentState, actualStepSize, derivativeFunc);
            stepCount++;

            // For adaptive integrators, check error and adjust step size
            if (config.Integrator != IntegratorType.RungeKutta4)
            {
                var tolerance = config.RelativeTolerance * Math.Max(currentState.Radius, 1e6) + config.AbsoluteTolerance;

                if (StepSizeController.ShouldRejectStep(errorEstimate, tolerance))
                {
                    // Reject step and reduce step size
                    stepSize = StepSizeController.ComputeNewStepSize(
                        Math.Abs(stepSize), errorEstimate, tolerance, 4,
                        config.MinStepSizeSeconds, config.MaxStepSizeSeconds);
                    continue;
                }

                // Accept step and compute new step size
                stepSize = StepSizeController.ComputeNewStepSize(
                    Math.Abs(stepSize), errorEstimate, tolerance, 4,
                    config.MinStepSizeSeconds, config.MaxStepSizeSeconds);
            }

            currentState = newState;

            // Output based on output mode
            if (config.OutputMode == OutputMode.IntegrationStep)
            {
                states.Add(currentState);
            }
            else if (config.OutputMode == OutputMode.FixedStep)
            {
                // Check if we've passed the next output time
                while ((direction > 0 && currentState.Epoch >= nextOutputTime) ||
                       (direction < 0 && currentState.Epoch <= nextOutputTime))
                {
                    // Interpolate to exact output time if needed
                    states.Add(currentState); // Simplified - could interpolate
                    nextOutputTime = nextOutputTime.AddSeconds(config.OutputStepSizeSeconds * direction);
                }
            }
        }
    }

    public async Task<Result<PropagationResult>> GetResultAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Results.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return Result<PropagationResult>.Failure(Error.NotFound("PropagationResult", id.ToString()));
        }
        return Result<PropagationResult>.Success(result);
    }

    public async Task<Result<IReadOnlyList<PropagationResult>>> GetRecentResultsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var results = await _unitOfWork.Results.GetRecentAsync(count, cancellationToken);
        return Result<IReadOnlyList<PropagationResult>>.Success(results);
    }
}
