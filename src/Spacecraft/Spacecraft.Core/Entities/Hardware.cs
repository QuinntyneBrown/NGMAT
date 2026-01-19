namespace Spacecraft.Core.Entities;

/// <summary>
/// Represents a thruster on the spacecraft.
/// </summary>
public sealed class Thruster
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ThrusterType Type { get; init; }
    public double ThrustN { get; init; }
    public double IspSeconds { get; init; }
    public double MassKg { get; init; }
    public FuelType FuelType { get; init; }
    public bool IsOperational { get; set; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Exhaust velocity in km/s (Isp * g0 / 1000).
    /// </summary>
    public double ExhaustVelocityKmPerSec => IspSeconds * 9.80665 / 1000.0;

    /// <summary>
    /// Mass flow rate in kg/s.
    /// </summary>
    public double MassFlowRateKgPerSec => ThrustN / (IspSeconds * 9.80665);

    public static Thruster Create(
        Guid spacecraftId,
        string name,
        ThrusterType type,
        double thrustN,
        double ispSeconds,
        double massKg,
        FuelType fuelType)
    {
        return new Thruster
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraftId,
            Name = name,
            Type = type,
            ThrustN = thrustN,
            IspSeconds = ispSeconds,
            MassKg = massKg,
            FuelType = fuelType,
            IsOperational = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Represents a fuel tank on the spacecraft.
/// </summary>
public sealed class FuelTank
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public string Name { get; init; } = string.Empty;
    public FuelType FuelType { get; init; }
    public double CapacityKg { get; init; }
    public double CurrentMassKg { get; set; }
    public double PressurePa { get; init; }
    public double MassKg { get; init; } // Tank dry mass
    public DateTime CreatedAt { get; init; }

    public double FillPercentage => CapacityKg > 0 ? CurrentMassKg / CapacityKg * 100 : 0;
    public bool IsEmpty => CurrentMassKg <= 0;
    public bool IsFull => CurrentMassKg >= CapacityKg;

    public static FuelTank Create(
        Guid spacecraftId,
        string name,
        FuelType fuelType,
        double capacityKg,
        double initialMassKg,
        double pressurePa,
        double tankMassKg)
    {
        return new FuelTank
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraftId,
            Name = name,
            FuelType = fuelType,
            CapacityKg = capacityKg,
            CurrentMassKg = initialMassKg,
            PressurePa = pressurePa,
            MassKg = tankMassKg,
            CreatedAt = DateTime.UtcNow
        };
    }

    public double Consume(double amountKg)
    {
        var consumed = Math.Min(amountKg, CurrentMassKg);
        CurrentMassKg -= consumed;
        return consumed;
    }
}

/// <summary>
/// Represents a solar panel array on the spacecraft.
/// </summary>
public sealed class SolarPanel
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double AreaM2 { get; init; }
    public double EfficiencyPercent { get; init; }
    public double MaxPowerWatts { get; init; }
    public double MassKg { get; init; }
    public bool IsDeployed { get; set; }
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Power output at 1 AU from Sun (1361 W/m²).
    /// </summary>
    public double PowerAt1AuWatts => IsDeployed ? AreaM2 * 1361 * EfficiencyPercent / 100.0 : 0;

    public static SolarPanel Create(
        Guid spacecraftId,
        string name,
        double areaM2,
        double efficiencyPercent,
        double maxPowerWatts,
        double massKg)
    {
        return new SolarPanel
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraftId,
            Name = name,
            AreaM2 = areaM2,
            EfficiencyPercent = efficiencyPercent,
            MaxPowerWatts = maxPowerWatts,
            MassKg = massKg,
            IsDeployed = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Represents a battery on the spacecraft.
/// </summary>
public sealed class Battery
{
    public Guid Id { get; init; }
    public Guid SpacecraftId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double CapacityWattHours { get; init; }
    public double CurrentChargeWattHours { get; set; }
    public double MassKg { get; init; }
    public BatteryType Type { get; init; }
    public DateTime CreatedAt { get; init; }

    public double StateOfChargePercent => CapacityWattHours > 0 ? CurrentChargeWattHours / CapacityWattHours * 100 : 0;

    public static Battery Create(
        Guid spacecraftId,
        string name,
        double capacityWattHours,
        double initialChargeWattHours,
        double massKg,
        BatteryType type)
    {
        return new Battery
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraftId,
            Name = name,
            CapacityWattHours = capacityWattHours,
            CurrentChargeWattHours = initialChargeWattHours,
            MassKg = massKg,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum ThrusterType
{
    Chemical,
    ElectricIonThruster,
    ElectricHallThruster,
    ColdGas,
    SolidRocket,
    NuclearThermal
}

public enum FuelType
{
    Hydrazine,
    Xenon,
    Bipropellant,
    SolidFuel,
    ColdGasNitrogen,
    MMH_NTO,
    LH2_LOX
}

public enum BatteryType
{
    LithiumIon,
    NickelHydrogen,
    NickelCadmium,
    SilverZinc
}
