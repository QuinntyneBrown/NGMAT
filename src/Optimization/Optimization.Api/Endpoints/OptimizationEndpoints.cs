using Microsoft.AspNetCore.Mvc;
using Optimization.Core.Entities;
using Optimization.Core.Services;

namespace Optimization.Api.Endpoints;

public static class OptimizationEndpoints
{
    public static IEndpointRouteBuilder MapOptimizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/optimization")
            .WithTags("Optimization")
            .RequireAuthorization();

        // Job management
        group.MapGet("/jobs", GetAllJobs)
            .WithName("GetAllOptimizationJobs")
            .WithSummary("Get all optimization jobs");

        group.MapGet("/jobs/{id:guid}", GetJob)
            .WithName("GetOptimizationJob")
            .WithSummary("Get an optimization job by ID");

        group.MapGet("/jobs/status/{status}", GetJobsByStatus)
            .WithName("GetOptimizationJobsByStatus")
            .WithSummary("Get optimization jobs by status");

        group.MapGet("/jobs/mission/{missionId:guid}", GetJobsByMission)
            .WithName("GetOptimizationJobsByMission")
            .WithSummary("Get optimization jobs for a mission");

        group.MapPost("/jobs", CreateJob)
            .WithName("CreateOptimizationJob")
            .WithSummary("Create a new optimization job");

        group.MapPost("/jobs/{id:guid}/cancel", CancelJob)
            .WithName("CancelOptimizationJob")
            .WithSummary("Cancel a running optimization job");

        group.MapDelete("/jobs/{id:guid}", DeleteJob)
            .WithName("DeleteOptimizationJob")
            .WithSummary("Delete an optimization job");

        // Direct optimization (stateless)
        group.MapPost("/minimize", MinimizeFunction)
            .WithName("MinimizeFunction")
            .WithSummary("Minimize a polynomial function directly");

        group.MapPost("/roots", FindRoots)
            .WithName("FindRoots")
            .WithSummary("Find roots of a system of polynomial equations");

        // Benchmark and test functions
        group.MapPost("/benchmark/rosenbrock", OptimizeRosenbrock)
            .WithName("OptimizeRosenbrock")
            .WithSummary("Optimize the Rosenbrock test function");

        group.MapPost("/benchmark/sphere", OptimizeSphere)
            .WithName("OptimizeSphere")
            .WithSummary("Optimize the sphere test function");

        group.MapPost("/benchmark/rastrigin", OptimizeRastrigin)
            .WithName("OptimizeRastrigin")
            .WithSummary("Optimize the Rastrigin test function");

        // Algorithm information
        group.MapGet("/algorithms", GetAvailableAlgorithms)
            .WithName("GetAvailableAlgorithms")
            .WithSummary("Get list of available optimization algorithms");

        return app;
    }

    // Job management handlers

    private static async Task<IResult> GetAllJobs(
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllJobsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(OptimizationJobResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetJob(
        Guid id,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetJobAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound")
                ? Results.NotFound(result.Error.Message)
                : Results.Problem(result.Error.Message);
        }
        return Results.Ok(OptimizationJobResponse.FromEntity(result.Value!));
    }

    private static async Task<IResult> GetJobsByStatus(
        string status,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OptimizationStatus>(status, true, out var parsedStatus))
        {
            return Results.BadRequest($"Invalid status: {status}");
        }

        var result = await service.GetJobsByStatusAsync(parsedStatus, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value!.Select(OptimizationJobResponse.FromEntity))
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetJobsByMission(
        Guid missionId,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllJobsAsync(cancellationToken);
        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }

        var missionJobs = result.Value!.Where(j => j.MissionId == missionId).ToList();
        return Results.Ok(missionJobs.Select(OptimizationJobResponse.FromEntity));
    }

    private static async Task<IResult> CreateJob(
        [FromBody] CreateOptimizationJobRequest request,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var designVariables = request.DesignVariables
            .Select(dv => (dv.Name, dv.InitialValue, dv.LowerBound, dv.UpperBound))
            .ToList();

        var result = await service.CreateJobAsync(
            request.Name,
            request.Algorithm,
            request.Objective,
            request.CreatedByUserId,
            designVariables,
            request.MaxIterations,
            request.ConvergenceTolerance,
            request.Description,
            request.MissionId,
            cancellationToken);

        if (result.IsFailure)
        {
            return Results.Problem(result.Error.Message);
        }
        return Results.Created($"/api/optimization/jobs/{result.Value!.Id}", OptimizationJobResponse.FromEntity(result.Value));
    }

    private static async Task<IResult> CancelJob(
        Guid id,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CancelJobAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { Message = "Job cancelled successfully" })
            : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> DeleteJob(
        Guid id,
        [FromServices] OptimizationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteJobAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error.Message);
    }

    // Direct optimization handlers

    private static IResult MinimizeFunction(
        [FromBody] MinimizeFunctionRequest request,
        [FromServices] OptimizationService service)
    {
        // Create objective function from polynomial coefficients
        ObjectiveFunction objective = x =>
        {
            double result = 0;
            for (int i = 0; i < request.Coefficients.Length && i < x.Length; i++)
            {
                result += request.Coefficients[i] * x[i] * x[i];
            }
            if (request.LinearCoefficients != null)
            {
                for (int i = 0; i < request.LinearCoefficients.Length && i < x.Length; i++)
                {
                    result += request.LinearCoefficients[i] * x[i];
                }
            }
            return result + (request.Constant ?? 0);
        };

        var result = service.MinimizeFunction(
            objective,
            request.InitialGuess,
            request.LowerBounds,
            request.UpperBounds,
            request.Algorithm,
            request.MaxIterations,
            request.Tolerance);

        return result.IsSuccess
            ? Results.Ok(OptimizationResultResponse.FromResult(result.Value!))
            : Results.Problem(result.Error.Message);
    }

    private static IResult FindRoots(
        [FromBody] FindRootsRequest request,
        [FromServices] OptimizationService service)
    {
        // Create system of equations from polynomial coefficients
        Func<double[], double[]> equations = x =>
        {
            var residuals = new double[request.EquationCoefficients.Length];
            for (int eq = 0; eq < request.EquationCoefficients.Length; eq++)
            {
                var coeffs = request.EquationCoefficients[eq];
                double r = 0;
                for (int i = 0; i < coeffs.Length && i < x.Length; i++)
                {
                    r += coeffs[i] * x[i];
                }
                residuals[eq] = r - (request.RightHandSides?[eq] ?? 0);
            }
            return residuals;
        };

        var result = service.FindRoots(
            equations,
            request.InitialGuess,
            request.LowerBounds,
            request.UpperBounds,
            request.MaxIterations,
            request.Tolerance);

        return result.IsSuccess
            ? Results.Ok(new RootFindingResponse(result.Value!, true, "Roots found successfully"))
            : Results.Problem(result.Error.Message);
    }

    // Benchmark handlers

    private static IResult OptimizeRosenbrock(
        [FromBody] BenchmarkOptimizationRequest request,
        [FromServices] OptimizationService service)
    {
        // Rosenbrock function: f(x,y) = (1-x)^2 + 100*(y-x^2)^2
        // Minimum at (1, 1) with f(1,1) = 0
        ObjectiveFunction rosenbrock = x =>
        {
            double sum = 0;
            for (int i = 0; i < x.Length - 1; i++)
            {
                sum += 100 * Math.Pow(x[i + 1] - x[i] * x[i], 2) + Math.Pow(1 - x[i], 2);
            }
            return sum;
        };

        var initialGuess = request.InitialGuess ?? new double[] { -1.0, -1.0 };
        var lowerBounds = request.LowerBounds ?? Enumerable.Repeat(-5.0, initialGuess.Length).ToArray();
        var upperBounds = request.UpperBounds ?? Enumerable.Repeat(5.0, initialGuess.Length).ToArray();

        var result = service.MinimizeFunction(
            rosenbrock,
            initialGuess,
            lowerBounds,
            upperBounds,
            request.Algorithm,
            request.MaxIterations,
            request.Tolerance);

        return result.IsSuccess
            ? Results.Ok(new BenchmarkResponse(
                "Rosenbrock",
                OptimizationResultResponse.FromResult(result.Value!),
                new double[] { 1.0, 1.0 },
                0.0))
            : Results.Problem(result.Error.Message);
    }

    private static IResult OptimizeSphere(
        [FromBody] BenchmarkOptimizationRequest request,
        [FromServices] OptimizationService service)
    {
        // Sphere function: f(x) = sum(x_i^2)
        // Minimum at origin with f(0) = 0
        ObjectiveFunction sphere = x => x.Sum(xi => xi * xi);

        var initialGuess = request.InitialGuess ?? new double[] { 5.0, 5.0, 5.0 };
        var lowerBounds = request.LowerBounds ?? Enumerable.Repeat(-10.0, initialGuess.Length).ToArray();
        var upperBounds = request.UpperBounds ?? Enumerable.Repeat(10.0, initialGuess.Length).ToArray();

        var result = service.MinimizeFunction(
            sphere,
            initialGuess,
            lowerBounds,
            upperBounds,
            request.Algorithm,
            request.MaxIterations,
            request.Tolerance);

        return result.IsSuccess
            ? Results.Ok(new BenchmarkResponse(
                "Sphere",
                OptimizationResultResponse.FromResult(result.Value!),
                Enumerable.Repeat(0.0, initialGuess.Length).ToArray(),
                0.0))
            : Results.Problem(result.Error.Message);
    }

    private static IResult OptimizeRastrigin(
        [FromBody] BenchmarkOptimizationRequest request,
        [FromServices] OptimizationService service)
    {
        // Rastrigin function: f(x) = 10n + sum(x_i^2 - 10*cos(2*pi*x_i))
        // Minimum at origin with f(0) = 0
        ObjectiveFunction rastrigin = x =>
        {
            int n = x.Length;
            double sum = 10 * n;
            foreach (var xi in x)
            {
                sum += xi * xi - 10 * Math.Cos(2 * Math.PI * xi);
            }
            return sum;
        };

        var initialGuess = request.InitialGuess ?? new double[] { 2.5, 2.5 };
        var lowerBounds = request.LowerBounds ?? Enumerable.Repeat(-5.12, initialGuess.Length).ToArray();
        var upperBounds = request.UpperBounds ?? Enumerable.Repeat(5.12, initialGuess.Length).ToArray();

        var result = service.MinimizeFunction(
            rastrigin,
            initialGuess,
            lowerBounds,
            upperBounds,
            request.Algorithm,
            request.MaxIterations,
            request.Tolerance);

        return result.IsSuccess
            ? Results.Ok(new BenchmarkResponse(
                "Rastrigin",
                OptimizationResultResponse.FromResult(result.Value!),
                Enumerable.Repeat(0.0, initialGuess.Length).ToArray(),
                0.0))
            : Results.Problem(result.Error.Message);
    }

    private static IResult GetAvailableAlgorithms()
    {
        var algorithms = new[]
        {
            new AlgorithmInfo(
                OptimizationAlgorithm.NelderMead.ToString(),
                "Nelder-Mead Simplex",
                "Derivative-free simplex method for unconstrained optimization",
                false,
                true),
            new AlgorithmInfo(
                OptimizationAlgorithm.GradientDescent.ToString(),
                "Gradient Descent",
                "First-order iterative optimization using numerical gradients",
                true,
                true),
            new AlgorithmInfo(
                OptimizationAlgorithm.ParticleSwarm.ToString(),
                "Particle Swarm Optimization",
                "Population-based stochastic optimization inspired by swarm behavior",
                false,
                true),
            new AlgorithmInfo(
                OptimizationAlgorithm.GeneticAlgorithm.ToString(),
                "Genetic Algorithm",
                "Evolutionary algorithm using selection, crossover, and mutation",
                false,
                true),
            new AlgorithmInfo(
                OptimizationAlgorithm.SequentialQuadraticProgramming.ToString(),
                "Sequential Quadratic Programming",
                "Constrained optimization using quadratic subproblems",
                true,
                false),
            new AlgorithmInfo(
                OptimizationAlgorithm.DifferentialCorrection.ToString(),
                "Differential Correction",
                "Iterative refinement for trajectory optimization",
                true,
                false)
        };

        return Results.Ok(algorithms);
    }
}

// Request DTOs

public record CreateOptimizationJobRequest(
    string Name,
    OptimizationAlgorithm Algorithm,
    OptimizationObjective Objective,
    string CreatedByUserId,
    List<DesignVariableRequest> DesignVariables,
    int MaxIterations = 1000,
    double ConvergenceTolerance = 1e-8,
    string? Description = null,
    Guid? MissionId = null);

public record DesignVariableRequest(
    string Name,
    double InitialValue,
    double LowerBound,
    double UpperBound);

public record MinimizeFunctionRequest(
    double[] Coefficients,
    double[] InitialGuess,
    double[] LowerBounds,
    double[] UpperBounds,
    OptimizationAlgorithm Algorithm = OptimizationAlgorithm.NelderMead,
    int MaxIterations = 1000,
    double Tolerance = 1e-8,
    double[]? LinearCoefficients = null,
    double? Constant = null);

public record FindRootsRequest(
    double[][] EquationCoefficients,
    double[] InitialGuess,
    double[] LowerBounds,
    double[] UpperBounds,
    int MaxIterations = 100,
    double Tolerance = 1e-10,
    double[]? RightHandSides = null);

public record BenchmarkOptimizationRequest(
    OptimizationAlgorithm Algorithm = OptimizationAlgorithm.NelderMead,
    int MaxIterations = 1000,
    double Tolerance = 1e-8,
    double[]? InitialGuess = null,
    double[]? LowerBounds = null,
    double[]? UpperBounds = null);

// Response DTOs

public record OptimizationJobResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? MissionId,
    string Algorithm,
    string Objective,
    string Status,
    int MaxIterations,
    double ConvergenceTolerance,
    int CurrentIteration,
    double CurrentCost,
    double BestCost,
    double? ConvergenceRate,
    List<DesignVariableResponse> DesignVariables,
    double[]? BestSolution,
    double[]? FinalSolution,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    long ComputationTimeMs,
    DateTime CreatedAt,
    string CreatedByUserId)
{
    public static OptimizationJobResponse FromEntity(OptimizationJob job) => new(
        job.Id,
        job.Name,
        job.Description,
        job.MissionId,
        job.Algorithm.ToString(),
        job.Objective.ToString(),
        job.Status.ToString(),
        job.MaxIterations,
        job.ConvergenceTolerance,
        job.CurrentIteration,
        job.CurrentCost,
        job.BestCost,
        job.ConvergenceRate,
        job.DesignVariables.Select(dv => new DesignVariableResponse(
            dv.Name, dv.InitialValue, dv.LowerBound, dv.UpperBound)).ToList(),
        job.BestSolution,
        job.FinalSolution,
        job.StartedAt,
        job.CompletedAt,
        job.ComputationTimeMs,
        job.CreatedAt,
        job.CreatedByUserId);
}

public record DesignVariableResponse(
    string Name,
    double InitialValue,
    double LowerBound,
    double UpperBound);

public record OptimizationResultResponse(
    double[] Solution,
    double ObjectiveValue,
    int Iterations,
    bool Converged,
    string TerminationReason)
{
    public static OptimizationResultResponse FromResult(OptimizationResult result) => new(
        result.Solution,
        result.ObjectiveValue,
        result.Iterations,
        result.Converged,
        result.TerminationReason);
}

public record RootFindingResponse(
    double[] Roots,
    bool Converged,
    string Message);

public record BenchmarkResponse(
    string FunctionName,
    OptimizationResultResponse Result,
    double[] KnownOptimum,
    double KnownMinimum);

public record AlgorithmInfo(
    string Name,
    string DisplayName,
    string Description,
    bool RequiresGradient,
    bool Implemented);
