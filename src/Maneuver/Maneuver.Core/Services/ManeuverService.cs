using Maneuver.Core.Entities;
using Maneuver.Core.Events;
using Maneuver.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Maneuver.Core.Services;

public sealed class ManeuverService
{
    private readonly IManeuverUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public ManeuverService(IManeuverUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    // CRUD Operations

    public async Task<Result<ManeuverPlan>> CreateImpulsiveManeuverAsync(
        string name,
        Guid spacecraftId,
        DateTime plannedEpoch,
        double deltaVx,
        double deltaVy,
        double deltaVz,
        CoordinateFrame frame,
        double spacecraftMassKg,
        double specificImpulseS,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null,
        CancellationToken cancellationToken = default)
    {
        var plan = ManeuverPlan.CreateImpulsive(
            name, spacecraftId, plannedEpoch, deltaVx, deltaVy, deltaVz,
            frame, spacecraftMassKg, specificImpulseS, createdByUserId, description, missionId);

        await _unitOfWork.ManeuverPlans.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ManeuverCreatedEvent
        {
            ManeuverPlanId = plan.Id,
            Name = plan.Name,
            SpacecraftId = plan.SpacecraftId,
            ManeuverType = plan.Type.ToString(),
            PlannedEpoch = plan.PlannedEpoch,
            DeltaVMps = plan.DeltaVMagnitude,
            EstimatedFuelKg = plan.EstimatedFuelMassKg
        }, cancellationToken);

        return Result<ManeuverPlan>.Success(plan);
    }

    public async Task<Result<ManeuverPlan>> CreateFiniteManeuverAsync(
        string name,
        Guid spacecraftId,
        DateTime plannedEpoch,
        double thrustMagnitudeN,
        double burnDurationSeconds,
        double thrustDirectionX,
        double thrustDirectionY,
        double thrustDirectionZ,
        CoordinateFrame frame,
        double spacecraftMassKg,
        double specificImpulseS,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null,
        CancellationToken cancellationToken = default)
    {
        var plan = ManeuverPlan.CreateFinite(
            name, spacecraftId, plannedEpoch, thrustMagnitudeN, burnDurationSeconds,
            thrustDirectionX, thrustDirectionY, thrustDirectionZ, frame,
            spacecraftMassKg, specificImpulseS, createdByUserId, description, missionId);

        await _unitOfWork.ManeuverPlans.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ManeuverCreatedEvent
        {
            ManeuverPlanId = plan.Id,
            Name = plan.Name,
            SpacecraftId = plan.SpacecraftId,
            ManeuverType = plan.Type.ToString(),
            PlannedEpoch = plan.PlannedEpoch,
            DeltaVMps = plan.DeltaVMagnitude,
            EstimatedFuelKg = plan.EstimatedFuelMassKg
        }, cancellationToken);

        return Result<ManeuverPlan>.Success(plan);
    }

    public async Task<Result<ManeuverPlan>> GetManeuverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.ManeuverPlans.GetByIdAsync(id, cancellationToken);
        if (plan == null || plan.IsDeleted)
        {
            return Result<ManeuverPlan>.Failure(Error.NotFound("ManeuverPlan", id.ToString()));
        }
        return Result<ManeuverPlan>.Success(plan);
    }

    public async Task<Result<IReadOnlyList<ManeuverPlan>>> GetManeuversBySpacecraftAsync(
        Guid spacecraftId,
        CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.ManeuverPlans.GetBySpacecraftIdAsync(spacecraftId, cancellationToken);
        return Result<IReadOnlyList<ManeuverPlan>>.Success(plans);
    }

    public async Task<Result<IReadOnlyList<ManeuverPlan>>> GetManeuversByMissionAsync(
        Guid missionId,
        CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.ManeuverPlans.GetByMissionIdAsync(missionId, cancellationToken);
        return Result<IReadOnlyList<ManeuverPlan>>.Success(plans);
    }

    public async Task<Result<IReadOnlyList<ManeuverPlan>>> GetAllManeuversAsync(
        CancellationToken cancellationToken = default)
    {
        var plans = await _unitOfWork.ManeuverPlans.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<ManeuverPlan>>.Success(plans);
    }

    public async Task<Result> ScheduleManeuverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.ManeuverPlans.GetByIdAsync(id, cancellationToken);
        if (plan == null || plan.IsDeleted)
        {
            return Result.Failure(Error.NotFound("ManeuverPlan", id.ToString()));
        }

        try
        {
            plan.Schedule();
            await _unitOfWork.ManeuverPlans.UpdateAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new ManeuverScheduledEvent
            {
                ManeuverPlanId = plan.Id,
                SpacecraftId = plan.SpacecraftId,
                PlannedEpoch = plan.PlannedEpoch
            }, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }

    public async Task<Result> CancelManeuverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.ManeuverPlans.GetByIdAsync(id, cancellationToken);
        if (plan == null || plan.IsDeleted)
        {
            return Result.Failure(Error.NotFound("ManeuverPlan", id.ToString()));
        }

        try
        {
            plan.Cancel();
            await _unitOfWork.ManeuverPlans.UpdateAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(new ManeuverCancelledEvent
            {
                ManeuverPlanId = plan.Id,
                SpacecraftId = plan.SpacecraftId
            }, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }

    public async Task<Result> DeleteManeuverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _unitOfWork.ManeuverPlans.GetByIdAsync(id, cancellationToken);
        if (plan == null)
        {
            return Result.Failure(Error.NotFound("ManeuverPlan", id.ToString()));
        }

        plan.Delete();
        await _unitOfWork.ManeuverPlans.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new ManeuverDeletedEvent
        {
            ManeuverPlanId = plan.Id
        }, cancellationToken);

        return Result.Success();
    }

    // Transfer Orbit Calculations

    public Result<TransferOrbitResult> CalculateHohmannTransfer(
        double initialRadiusM,
        double finalRadiusM,
        double centralBodyGM = 3.986004418e14)
    {
        try
        {
            var transfer = HohmannTransfer.Calculate(initialRadiusM, finalRadiusM, centralBodyGM);

            var burns = new List<TransferBurn>
            {
                new(1, "Departure (periapsis)", transfer.DeltaV1Mps, 0),
                new(2, "Arrival (apoapsis)", transfer.DeltaV2Mps, transfer.TransferTimeSeconds)
            };

            var result = new TransferOrbitResult(
                "Hohmann",
                initialRadiusM,
                finalRadiusM,
                burns,
                transfer.TotalDeltaVMps,
                transfer.TransferTimeSeconds);

            // Fire event asynchronously (fire and forget for calculations)
            _ = _eventPublisher.PublishAsync(new TransferOrbitCalculatedEvent
            {
                CalculationId = result.Id,
                TransferType = "Hohmann",
                TotalDeltaVMps = result.TotalDeltaVMps,
                TransferTimeSeconds = result.TransferTimeSeconds,
                NumberOfBurns = 2
            });

            return Result<TransferOrbitResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TransferOrbitResult>.Failure(Error.Internal($"Hohmann transfer calculation failed: {ex.Message}"));
        }
    }

    public Result<TransferOrbitResult> CalculateBiEllipticTransfer(
        double initialRadiusM,
        double finalRadiusM,
        double intermediateRadiusM,
        double centralBodyGM = 3.986004418e14)
    {
        try
        {
            var transfer = BiEllipticTransfer.Calculate(initialRadiusM, finalRadiusM, intermediateRadiusM, centralBodyGM);

            var burns = new List<TransferBurn>
            {
                new(1, "First burn (departure)", transfer.DeltaV1Mps, 0),
                new(2, "Second burn (intermediate apoapsis)", transfer.DeltaV2Mps, transfer.TransferTimeSeconds / 2),
                new(3, "Third burn (arrival)", transfer.DeltaV3Mps, transfer.TransferTimeSeconds)
            };

            var result = new TransferOrbitResult(
                "BiElliptic",
                initialRadiusM,
                finalRadiusM,
                burns,
                transfer.TotalDeltaVMps,
                transfer.TransferTimeSeconds,
                intermediateRadiusM);

            _ = _eventPublisher.PublishAsync(new TransferOrbitCalculatedEvent
            {
                CalculationId = result.Id,
                TransferType = "BiElliptic",
                TotalDeltaVMps = result.TotalDeltaVMps,
                TransferTimeSeconds = result.TransferTimeSeconds,
                NumberOfBurns = 3
            });

            return Result<TransferOrbitResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TransferOrbitResult>.Failure(Error.Internal($"Bi-elliptic transfer calculation failed: {ex.Message}"));
        }
    }

    public Result<TransferOrbitResult> CalculatePlaneChange(
        double orbitRadiusM,
        double inclinationChangeDeg,
        bool burnAtAscendingNode = true,
        double centralBodyGM = 3.986004418e14)
    {
        try
        {
            var maneuver = PlaneChangeManeuver.Calculate(orbitRadiusM, inclinationChangeDeg, burnAtAscendingNode, centralBodyGM);

            var burns = new List<TransferBurn>
            {
                new(1, burnAtAscendingNode ? "Ascending node" : "Descending node", maneuver.DeltaVMps, 0)
            };

            var result = new TransferOrbitResult(
                "PlaneChange",
                orbitRadiusM,
                orbitRadiusM,
                burns,
                maneuver.DeltaVMps,
                0);

            _ = _eventPublisher.PublishAsync(new TransferOrbitCalculatedEvent
            {
                CalculationId = result.Id,
                TransferType = "PlaneChange",
                TotalDeltaVMps = result.TotalDeltaVMps,
                TransferTimeSeconds = 0,
                NumberOfBurns = 1
            });

            return Result<TransferOrbitResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TransferOrbitResult>.Failure(Error.Internal($"Plane change calculation failed: {ex.Message}"));
        }
    }

    public Result<double> CalculateFuelMass(
        double initialMassKg,
        double deltaVMps,
        double specificImpulseS)
    {
        try
        {
            var fuelMass = ManeuverPlan.CalculateFuelMass(initialMassKg, deltaVMps, specificImpulseS);
            return Result<double>.Success(fuelMass);
        }
        catch (Exception ex)
        {
            return Result<double>.Failure(Error.Internal($"Fuel mass calculation failed: {ex.Message}"));
        }
    }

    public Result<TransferOrbitResult> CompareTransfers(
        double initialRadiusM,
        double finalRadiusM,
        double centralBodyGM = 3.986004418e14)
    {
        var hohmann = HohmannTransfer.Calculate(initialRadiusM, finalRadiusM, centralBodyGM);

        // Check if bi-elliptic might be more efficient
        var radiusRatio = Math.Max(initialRadiusM, finalRadiusM) / Math.Min(initialRadiusM, finalRadiusM);

        var burns = new List<TransferBurn>
        {
            new(1, "Hohmann - Departure", hohmann.DeltaV1Mps, 0),
            new(2, "Hohmann - Arrival", hohmann.DeltaV2Mps, hohmann.TransferTimeSeconds)
        };

        string transferType = "Hohmann";
        double totalDeltaV = hohmann.TotalDeltaVMps;
        double transferTime = hohmann.TransferTimeSeconds;

        // For large radius ratios, bi-elliptic might be better
        if (radiusRatio > 11.94)
        {
            var intermediateRadius = BiEllipticTransfer.FindOptimalIntermediateRadius(initialRadiusM, finalRadiusM);
            var biElliptic = BiEllipticTransfer.Calculate(initialRadiusM, finalRadiusM, intermediateRadius, centralBodyGM);

            if (biElliptic.IsMoreEfficientThanHohmann)
            {
                burns.Clear();
                burns.Add(new(1, "Bi-elliptic - First burn", biElliptic.DeltaV1Mps, 0));
                burns.Add(new(2, "Bi-elliptic - Second burn", biElliptic.DeltaV2Mps, biElliptic.TransferTimeSeconds / 2));
                burns.Add(new(3, "Bi-elliptic - Third burn", biElliptic.DeltaV3Mps, biElliptic.TransferTimeSeconds));

                transferType = "BiElliptic (recommended)";
                totalDeltaV = biElliptic.TotalDeltaVMps;
                transferTime = biElliptic.TransferTimeSeconds;
            }
        }

        return Result<TransferOrbitResult>.Success(new TransferOrbitResult(
            transferType,
            initialRadiusM,
            finalRadiusM,
            burns,
            totalDeltaV,
            transferTime));
    }
}
