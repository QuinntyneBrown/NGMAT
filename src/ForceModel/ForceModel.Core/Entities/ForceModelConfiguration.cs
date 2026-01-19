namespace ForceModel.Core.Entities;

public enum GravityModelType
{
    PointMass,
    J2Only,
    J2J3,
    SphericalHarmonics
}

public enum AtmosphereModelType
{
    None,
    Exponential,
    HarrisPriester,
    NRLMSISE00,
    JacchiaRoberts
}

public enum SrpModelType
{
    None,
    CannonBall,
    BoxWing
}

public sealed class ForceModelConfiguration
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? MissionId { get; private set; }

    // Gravity settings
    public bool EnableCentralBodyGravity { get; private set; } = true;
    public GravityModelType GravityModel { get; private set; } = GravityModelType.J2Only;
    public int GravityDegree { get; private set; } = 2;
    public int GravityOrder { get; private set; } = 0;

    // Atmosphere settings
    public bool EnableAtmosphericDrag { get; private set; }
    public AtmosphereModelType AtmosphereModel { get; private set; } = AtmosphereModelType.Exponential;

    // SRP settings
    public bool EnableSolarRadiationPressure { get; private set; }
    public SrpModelType SrpModel { get; private set; } = SrpModelType.CannonBall;
    public bool EnableEclipsing { get; private set; } = true;

    // Third body settings
    public bool EnableThirdBodySun { get; private set; }
    public bool EnableThirdBodyMoon { get; private set; }
    public bool EnableThirdBodyPlanets { get; private set; }

    // Relativistic effects
    public bool EnableRelativisticCorrections { get; private set; }

    // Other perturbations
    public bool EnableSolidEarthTides { get; private set; }
    public bool EnableOceanTides { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private ForceModelConfiguration() { }

    public static ForceModelConfiguration Create(
        string name,
        string createdByUserId,
        string? description = null,
        Guid? missionId = null)
    {
        return new ForceModelConfiguration
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            MissionId = missionId,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };
    }

    public void SetGravitySettings(
        bool enable,
        GravityModelType model,
        int degree,
        int order)
    {
        EnableCentralBodyGravity = enable;
        GravityModel = model;
        GravityDegree = degree;
        GravityOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAtmosphereSettings(
        bool enable,
        AtmosphereModelType model)
    {
        EnableAtmosphericDrag = enable;
        AtmosphereModel = model;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSrpSettings(
        bool enable,
        SrpModelType model,
        bool enableEclipsing)
    {
        EnableSolarRadiationPressure = enable;
        SrpModel = model;
        EnableEclipsing = enableEclipsing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetThirdBodySettings(
        bool enableSun,
        bool enableMoon,
        bool enablePlanets)
    {
        EnableThirdBodySun = enableSun;
        EnableThirdBodyMoon = enableMoon;
        EnableThirdBodyPlanets = enablePlanets;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAdvancedSettings(
        bool enableRelativistic,
        bool enableSolidTides,
        bool enableOceanTides)
    {
        EnableRelativisticCorrections = enableRelativistic;
        EnableSolidEarthTides = enableSolidTides;
        EnableOceanTides = enableOceanTides;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class StandardForceModels
{
    public static ForceModelConfiguration CreateLowFidelity(string userId)
    {
        var config = ForceModelConfiguration.Create("Low Fidelity", userId, "Simple two-body with J2");
        config.SetGravitySettings(true, GravityModelType.J2Only, 2, 0);
        config.SetAtmosphereSettings(false, AtmosphereModelType.None);
        config.SetSrpSettings(false, SrpModelType.None, false);
        config.SetThirdBodySettings(false, false, false);
        return config;
    }

    public static ForceModelConfiguration CreateMediumFidelity(string userId)
    {
        var config = ForceModelConfiguration.Create("Medium Fidelity", userId, "LEO standard with drag and SRP");
        config.SetGravitySettings(true, GravityModelType.J2J3, 4, 4);
        config.SetAtmosphereSettings(true, AtmosphereModelType.Exponential);
        config.SetSrpSettings(true, SrpModelType.CannonBall, true);
        config.SetThirdBodySettings(true, true, false);
        return config;
    }

    public static ForceModelConfiguration CreateHighFidelity(string userId)
    {
        var config = ForceModelConfiguration.Create("High Fidelity", userId, "Full perturbation model");
        config.SetGravitySettings(true, GravityModelType.SphericalHarmonics, 70, 70);
        config.SetAtmosphereSettings(true, AtmosphereModelType.NRLMSISE00);
        config.SetSrpSettings(true, SrpModelType.BoxWing, true);
        config.SetThirdBodySettings(true, true, true);
        config.SetAdvancedSettings(true, true, true);
        return config;
    }
}
