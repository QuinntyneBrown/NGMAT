using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for unit conversions.
/// </summary>
public sealed class UnitConversionService
{
    private static readonly Dictionary<string, double> LengthToMeters = new()
    {
        ["m"] = 1.0,
        ["km"] = 1000.0,
        ["cm"] = 0.01,
        ["mm"] = 0.001,
        ["au"] = 149597870700.0,
        ["ft"] = 0.3048,
        ["mi"] = 1609.344,
        ["nmi"] = 1852.0,
        ["yd"] = 0.9144,
        ["in"] = 0.0254
    };

    private static readonly Dictionary<string, double> MassToKg = new()
    {
        ["kg"] = 1.0,
        ["g"] = 0.001,
        ["mg"] = 0.000001,
        ["lbm"] = 0.45359237,
        ["oz"] = 0.028349523125,
        ["t"] = 1000.0 // metric ton
    };

    private static readonly Dictionary<string, double> TimeToSeconds = new()
    {
        ["s"] = 1.0,
        ["ms"] = 0.001,
        ["min"] = 60.0,
        ["hr"] = 3600.0,
        ["day"] = 86400.0,
        ["week"] = 604800.0,
        ["year"] = 31557600.0 // Julian year
    };

    private static readonly Dictionary<string, double> AngleToRadians = new()
    {
        ["rad"] = 1.0,
        ["deg"] = Math.PI / 180.0,
        ["arcmin"] = Math.PI / 10800.0,
        ["arcsec"] = Math.PI / 648000.0,
        ["grad"] = Math.PI / 200.0,
        ["rev"] = 2 * Math.PI
    };

    private static readonly Dictionary<string, double> VelocityToMps = new()
    {
        ["m/s"] = 1.0,
        ["km/s"] = 1000.0,
        ["km/hr"] = 1.0 / 3.6,
        ["ft/s"] = 0.3048,
        ["mph"] = 0.44704,
        ["kn"] = 0.514444 // knots
    };

    private static readonly Dictionary<string, double> ForceToNewtons = new()
    {
        ["n"] = 1.0,
        ["kn"] = 1000.0,
        ["lbf"] = 4.44822162,
        ["dyn"] = 0.00001,
        ["kgf"] = 9.80665
    };

    public Result<double> ConvertLength(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, LengthToMeters, "Length");
    }

    public Result<double> ConvertMass(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, MassToKg, "Mass");
    }

    public Result<double> ConvertTime(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, TimeToSeconds, "Time");
    }

    public Result<double> ConvertAngle(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, AngleToRadians, "Angle");
    }

    public Result<double> ConvertVelocity(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, VelocityToMps, "Velocity");
    }

    public Result<double> ConvertForce(double value, string fromUnit, string toUnit)
    {
        return Convert(value, fromUnit, toUnit, ForceToNewtons, "Force");
    }

    public Result<double> Convert(
        double value,
        string fromUnit,
        string toUnit,
        string unitType)
    {
        var conversions = unitType.ToLowerInvariant() switch
        {
            "length" => LengthToMeters,
            "mass" => MassToKg,
            "time" => TimeToSeconds,
            "angle" => AngleToRadians,
            "velocity" => VelocityToMps,
            "force" => ForceToNewtons,
            _ => null
        };

        if (conversions == null)
        {
            return Error.Validation($"Unknown unit type: {unitType}");
        }

        return Convert(value, fromUnit, toUnit, conversions, unitType);
    }

    public IReadOnlyList<string> GetSupportedUnits(string unitType)
    {
        var conversions = unitType.ToLowerInvariant() switch
        {
            "length" => LengthToMeters,
            "mass" => MassToKg,
            "time" => TimeToSeconds,
            "angle" => AngleToRadians,
            "velocity" => VelocityToMps,
            "force" => ForceToNewtons,
            _ => new Dictionary<string, double>()
        };

        return conversions.Keys.ToList();
    }

    private static Result<double> Convert(
        double value,
        string fromUnit,
        string toUnit,
        Dictionary<string, double> conversions,
        string unitType)
    {
        var fromLower = fromUnit.ToLowerInvariant();
        var toLower = toUnit.ToLowerInvariant();

        if (!conversions.TryGetValue(fromLower, out var fromFactor))
        {
            return Error.Validation($"Unknown {unitType} unit: {fromUnit}");
        }

        if (!conversions.TryGetValue(toLower, out var toFactor))
        {
            return Error.Validation($"Unknown {unitType} unit: {toUnit}");
        }

        // Convert to base unit, then to target unit
        var result = value * fromFactor / toFactor;
        return result;
    }
}
