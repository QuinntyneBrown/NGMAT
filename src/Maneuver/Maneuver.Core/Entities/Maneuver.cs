namespace Maneuver.Core.Entities;

public enum ManeuverType
{
    Impulsive,
    Finite,
    HohmannTransfer,
    BiEllipticTransfer,
    PlaneChange,
    Rendezvous,
    StationKeeping
}

public enum ManeuverStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Failed
}

public enum CoordinateFrame
{
    Inertial,   // Earth-Centered Inertial (ECI)
    VNB,        // Velocity-Normal-Binormal (along-track, cross-track, radial)
    LVLH,       // Local Vertical Local Horizontal
    RTN         // Radial-Transverse-Normal
}

public sealed class ManeuverPlan
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid SpacecraftId { get; private set; }
    public Guid? MissionId { get; private set; }
    public ManeuverType Type { get; private set; }
    public ManeuverStatus Status { get; private set; }
    public DateTime PlannedEpoch { get; private set; }
    public DateTime? ExecutedEpoch { get; private set; }

    // Delta-V components
    public double DeltaVx { get; private set; }
    public double DeltaVy { get; private set; }
    public double DeltaVz { get; private set; }
    public CoordinateFrame CoordinateFrame { get; private set; }

    // For finite burns
    public double? ThrustMagnitudeN { get; private set; }
    public double? BurnDurationSeconds { get; private set; }
    public double? SpecificImpulseS { get; private set; }

    // Fuel tracking
    public double EstimatedFuelMassKg { get; private set; }
    public double? ActualFuelMassKg { get; private set; }
    public double SpacecraftMassBeforeKg { get; private set; }
    public double SpacecraftMassAfterKg { get; private set; }

    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private ManeuverPlan() { }

    public static ManeuverPlan CreateImpulsive(
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
        Guid? missionId = null)
    {
        var deltaVMagnitude = Math.Sqrt(deltaVx * deltaVx + deltaVy * deltaVy + deltaVz * deltaVz);
        var estimatedFuelMass = CalculateFuelMass(spacecraftMassKg, deltaVMagnitude, specificImpulseS);

        return new ManeuverPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            SpacecraftId = spacecraftId,
            MissionId = missionId,
            Type = ManeuverType.Impulsive,
            Status = ManeuverStatus.Planned,
            PlannedEpoch = plannedEpoch,
            DeltaVx = deltaVx,
            DeltaVy = deltaVy,
            DeltaVz = deltaVz,
            CoordinateFrame = frame,
            SpecificImpulseS = specificImpulseS,
            EstimatedFuelMassKg = estimatedFuelMass,
            SpacecraftMassBeforeKg = spacecraftMassKg,
            SpacecraftMassAfterKg = spacecraftMassKg - estimatedFuelMass,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public static ManeuverPlan CreateFinite(
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
        Guid? missionId = null)
    {
        // Normalize thrust direction
        var mag = Math.Sqrt(thrustDirectionX * thrustDirectionX + thrustDirectionY * thrustDirectionY + thrustDirectionZ * thrustDirectionZ);
        var dirX = thrustDirectionX / mag;
        var dirY = thrustDirectionY / mag;
        var dirZ = thrustDirectionZ / mag;

        // Calculate mass flow rate and fuel consumption
        var g0 = 9.80665; // Standard gravity m/s^2
        var massFlowRate = thrustMagnitudeN / (specificImpulseS * g0); // kg/s
        var fuelMass = massFlowRate * burnDurationSeconds;

        // Calculate approximate delta-V using average mass
        var avgMass = spacecraftMassKg - fuelMass / 2;
        var acceleration = thrustMagnitudeN / avgMass;
        var deltaVMagnitude = acceleration * burnDurationSeconds;

        return new ManeuverPlan
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            SpacecraftId = spacecraftId,
            MissionId = missionId,
            Type = ManeuverType.Finite,
            Status = ManeuverStatus.Planned,
            PlannedEpoch = plannedEpoch,
            DeltaVx = deltaVMagnitude * dirX,
            DeltaVy = deltaVMagnitude * dirY,
            DeltaVz = deltaVMagnitude * dirZ,
            CoordinateFrame = frame,
            ThrustMagnitudeN = thrustMagnitudeN,
            BurnDurationSeconds = burnDurationSeconds,
            SpecificImpulseS = specificImpulseS,
            EstimatedFuelMassKg = fuelMass,
            SpacecraftMassBeforeKg = spacecraftMassKg,
            SpacecraftMassAfterKg = spacecraftMassKg - fuelMass,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void Schedule()
    {
        if (Status != ManeuverStatus.Planned)
            throw new InvalidOperationException($"Cannot schedule maneuver in {Status} status");

        Status = ManeuverStatus.Scheduled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartExecution(DateTime epoch)
    {
        if (Status != ManeuverStatus.Scheduled)
            throw new InvalidOperationException($"Cannot start maneuver in {Status} status");

        Status = ManeuverStatus.InProgress;
        ExecutedEpoch = epoch;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(double actualFuelMassKg)
    {
        if (Status != ManeuverStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete maneuver in {Status} status");

        Status = ManeuverStatus.Completed;
        ActualFuelMassKg = actualFuelMassKg;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ManeuverStatus.Completed || Status == ManeuverStatus.InProgress)
            throw new InvalidOperationException($"Cannot cancel maneuver in {Status} status");

        Status = ManeuverStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = ManeuverStatus.Failed;
        Description = $"{Description}\nFailure reason: {reason}".Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public double DeltaVMagnitude => Math.Sqrt(DeltaVx * DeltaVx + DeltaVy * DeltaVy + DeltaVz * DeltaVz);

    /// <summary>
    /// Calculates fuel mass using Tsiolkovsky rocket equation
    /// </summary>
    public static double CalculateFuelMass(double initialMassKg, double deltaVMps, double specificImpulseS)
    {
        var g0 = 9.80665; // Standard gravity m/s^2
        var exhaustVelocity = specificImpulseS * g0;
        var massRatio = Math.Exp(deltaVMps / exhaustVelocity);
        var finalMass = initialMassKg / massRatio;
        return initialMassKg - finalMass;
    }
}
