using Shared.Domain.Results;
using Shared.Messaging.Abstractions;
using Spacecraft.Core.Entities;
using Spacecraft.Core.Events;
using Spacecraft.Core.Interfaces;

namespace Spacecraft.Core.Services;

public sealed class SpacecraftService
{
    private readonly ISpacecraftUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public SpacecraftService(ISpacecraftUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<SpacecraftEntity>> CreateSpacecraftAsync(
        string name,
        Guid missionId,
        double dryMassKg,
        double fuelMassKg,
        double dragCoefficient,
        double dragAreaM2,
        double srpAreaM2,
        double reflectivityCoefficient,
        DateTime initialEpoch,
        double x, double y, double z,
        double vx, double vy, double vz,
        Guid coordinateFrameId,
        Guid userId,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        // Check name uniqueness within mission
        var exists = await _unitOfWork.Spacecraft.ExistsByNameAndMissionAsync(name, missionId, cancellationToken);
        if (exists)
        {
            return Error.Conflict($"A spacecraft with the name '{name}' already exists in this mission");
        }

        var spacecraft = SpacecraftEntity.Create(
            name, missionId, dryMassKg, fuelMassKg,
            dragCoefficient, dragAreaM2, srpAreaM2, reflectivityCoefficient,
            initialEpoch, x, y, z, vx, vy, vz,
            coordinateFrameId, userId, description);

        // Validate
        var validation = spacecraft.Validate();
        if (!validation.IsValid)
        {
            return Error.Validation(string.Join("; ", validation.Errors));
        }

        await _unitOfWork.Spacecraft.AddAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SpacecraftCreatedEvent
        {
            SpacecraftId = spacecraft.Id,
            Name = spacecraft.Name,
            MissionId = spacecraft.MissionId,
            DryMassKg = spacecraft.DryMassKg,
            FuelMassKg = spacecraft.FuelMassKg,
            CreatedByUserId = userId
        }, cancellationToken);

        return spacecraft;
    }

    public async Task<Result<SpacecraftEntity>> GetSpacecraftAsync(
        Guid spacecraftId,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdWithHardwareAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        return spacecraft;
    }

    public async Task<Result<IReadOnlyList<SpacecraftEntity>>> GetByMissionAsync(
        Guid missionId,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByMissionIdAsync(missionId, cancellationToken);
        return Result<IReadOnlyList<SpacecraftEntity>>.Success(spacecraft);
    }

    public async Task<Result<SpacecraftEntity>> UpdateSpacecraftAsync(
        Guid spacecraftId,
        Guid userId,
        string? name = null,
        string? description = null,
        double? dryMassKg = null,
        double? dragCoefficient = null,
        double? dragAreaM2 = null,
        double? srpAreaM2 = null,
        double? reflectivityCoefficient = null,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        // Check name uniqueness if changing
        if (name != null && name != spacecraft.Name)
        {
            var exists = await _unitOfWork.Spacecraft.ExistsByNameAndMissionAsync(name, spacecraft.MissionId, cancellationToken);
            if (exists)
            {
                return Error.Conflict($"A spacecraft with the name '{name}' already exists in this mission");
            }
        }

        spacecraft.Update(name, description, dryMassKg, dragCoefficient, dragAreaM2, srpAreaM2, reflectivityCoefficient);

        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SpacecraftUpdatedEvent
        {
            SpacecraftId = spacecraft.Id,
            Name = name,
            DryMassKg = dryMassKg,
            DragCoefficient = dragCoefficient,
            UpdatedByUserId = userId
        }, cancellationToken);

        return spacecraft;
    }

    public async Task<Result> DeleteSpacecraftAsync(
        Guid spacecraftId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        spacecraft.Delete();
        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SpacecraftDeletedEvent
        {
            SpacecraftId = spacecraft.Id,
            MissionId = spacecraft.MissionId,
            DeletedByUserId = userId
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SpacecraftState>> GetStateAtEpochAsync(
        Guid spacecraftId,
        DateTime epoch,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdWithStateHistoryAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        var state = spacecraft.GetStateAtEpoch(epoch);
        if (state == null)
        {
            return Error.NotFound("State", $"at epoch {epoch:O}");
        }

        return state;
    }

    public async Task<Result<SpacecraftEntity>> RecordStateAsync(
        Guid spacecraftId,
        DateTime epoch,
        double x, double y, double z,
        double vx, double vy, double vz,
        double fuelMassKg,
        Guid coordinateFrameId,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        var state = new SpacecraftState
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraftId,
            Epoch = epoch,
            X = x,
            Y = y,
            Z = z,
            Vx = vx,
            Vy = vy,
            Vz = vz,
            FuelMassKg = fuelMassKg,
            CoordinateFrameId = coordinateFrameId,
            RecordedAt = DateTime.UtcNow
        };

        await _unitOfWork.SpacecraftStates.AddAsync(state, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new StateRecordedEvent
        {
            SpacecraftId = spacecraftId,
            Epoch = epoch,
            X = x,
            Y = y,
            Z = z,
            Vx = vx,
            Vy = vy,
            Vz = vz
        }, cancellationToken);

        return spacecraft;
    }

    public async Task<Result<SpacecraftEntity>> ConsumeFuelAsync(
        Guid spacecraftId,
        double amountKg,
        Guid? maneuverId = null,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        if (amountKg > spacecraft.FuelMassKg)
        {
            return Error.Validation($"Insufficient fuel: requested {amountKg} kg, available {spacecraft.FuelMassKg} kg");
        }

        spacecraft.ConsumeFuel(amountKg);
        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new FuelConsumedEvent
        {
            SpacecraftId = spacecraftId,
            AmountKg = amountKg,
            RemainingKg = spacecraft.FuelMassKg,
            ManeuverId = maneuverId
        }, cancellationToken);

        return spacecraft;
    }

    public async Task<Result<SpacecraftEntity>> UpdateAttitudeAsync(
        Guid spacecraftId,
        AttitudeMode mode,
        double q0, double q1, double q2, double q3,
        double spinRate = 0,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        // Normalize quaternion
        var mag = Math.Sqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
        if (Math.Abs(mag - 1.0) > 0.01)
        {
            q0 /= mag;
            q1 /= mag;
            q2 /= mag;
            q3 /= mag;
        }

        spacecraft.UpdateAttitude(mode, q0, q1, q2, q3, spinRate);
        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new AttitudeChangedEvent
        {
            SpacecraftId = spacecraftId,
            Mode = mode,
            Q0 = q0,
            Q1 = q1,
            Q2 = q2,
            Q3 = q3,
            SpinRateRadPerSec = spinRate
        }, cancellationToken);

        return spacecraft;
    }

    public async Task<Result<Thruster>> AddThrusterAsync(
        Guid spacecraftId,
        string name,
        ThrusterType type,
        double thrustN,
        double ispSeconds,
        double massKg,
        FuelType fuelType,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdWithHardwareAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        var thruster = Thruster.Create(spacecraftId, name, type, thrustN, ispSeconds, massKg, fuelType);
        spacecraft.AddThruster(thruster);

        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new HardwareConfiguredEvent
        {
            SpacecraftId = spacecraftId,
            HardwareType = "Thruster",
            HardwareId = thruster.Id,
            HardwareName = thruster.Name
        }, cancellationToken);

        return thruster;
    }

    public async Task<Result<FuelTank>> AddFuelTankAsync(
        Guid spacecraftId,
        string name,
        FuelType fuelType,
        double capacityKg,
        double initialMassKg,
        double pressurePa,
        double tankMassKg,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdWithHardwareAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        var tank = FuelTank.Create(spacecraftId, name, fuelType, capacityKg, initialMassKg, pressurePa, tankMassKg);
        spacecraft.AddFuelTank(tank);

        await _unitOfWork.Spacecraft.UpdateAsync(spacecraft, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new HardwareConfiguredEvent
        {
            SpacecraftId = spacecraftId,
            HardwareType = "FuelTank",
            HardwareId = tank.Id,
            HardwareName = tank.Name
        }, cancellationToken);

        return tank;
    }

    public async Task<Result<ValidationResult>> ValidateSpacecraftAsync(
        Guid spacecraftId,
        CancellationToken cancellationToken = default)
    {
        var spacecraft = await _unitOfWork.Spacecraft.GetByIdWithHardwareAsync(spacecraftId, cancellationToken);
        if (spacecraft == null || spacecraft.IsDeleted)
        {
            return Error.NotFound("Spacecraft", spacecraftId.ToString());
        }

        var validation = spacecraft.Validate();
        return validation;
    }
}
