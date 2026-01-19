namespace Spacecraft.Core.Entities;

/// <summary>
/// Represents a spacecraft with physical properties and configuration.
/// </summary>
public sealed class SpacecraftEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid MissionId { get; private set; }

    // Mass properties
    public double DryMassKg { get; private set; }
    public double FuelMassKg { get; private set; }
    public double TotalMassKg => DryMassKg + FuelMassKg;

    // Aerodynamic properties
    public double DragCoefficient { get; private set; }
    public double DragAreaM2 { get; private set; }

    // Solar radiation pressure properties
    public double SrpAreaM2 { get; private set; }
    public double ReflectivityCoefficient { get; private set; }

    // Initial state
    public DateTime InitialEpoch { get; private set; }
    public double InitialX { get; private set; }
    public double InitialY { get; private set; }
    public double InitialZ { get; private set; }
    public double InitialVx { get; private set; }
    public double InitialVy { get; private set; }
    public double InitialVz { get; private set; }
    public Guid CoordinateFrameId { get; private set; }

    // Attitude
    public AttitudeMode AttitudeMode { get; private set; }
    public double AttitudeQ0 { get; private set; }
    public double AttitudeQ1 { get; private set; }
    public double AttitudeQ2 { get; private set; }
    public double AttitudeQ3 { get; private set; }
    public double SpinRateRadPerSec { get; private set; }

    // Moment of inertia tensor (diagonal elements for simplified model)
    public double MomentOfInertiaX { get; private set; }
    public double MomentOfInertiaY { get; private set; }
    public double MomentOfInertiaZ { get; private set; }

    // Metadata
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Hardware collections
    private readonly List<Thruster> _thrusters = new();
    public IReadOnlyCollection<Thruster> Thrusters => _thrusters.AsReadOnly();

    private readonly List<FuelTank> _fuelTanks = new();
    public IReadOnlyCollection<FuelTank> FuelTanks => _fuelTanks.AsReadOnly();

    private readonly List<SpacecraftState> _stateHistory = new();
    public IReadOnlyCollection<SpacecraftState> StateHistory => _stateHistory.AsReadOnly();

    private SpacecraftEntity() { }

    public static SpacecraftEntity Create(
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
        Guid createdByUserId,
        string? description = null)
    {
        var spacecraft = new SpacecraftEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            MissionId = missionId,
            DryMassKg = dryMassKg,
            FuelMassKg = fuelMassKg,
            DragCoefficient = dragCoefficient,
            DragAreaM2 = dragAreaM2,
            SrpAreaM2 = srpAreaM2,
            ReflectivityCoefficient = reflectivityCoefficient,
            InitialEpoch = initialEpoch,
            InitialX = x,
            InitialY = y,
            InitialZ = z,
            InitialVx = vx,
            InitialVy = vy,
            InitialVz = vz,
            CoordinateFrameId = coordinateFrameId,
            AttitudeMode = AttitudeMode.NadirPointing,
            AttitudeQ0 = 1,
            AttitudeQ1 = 0,
            AttitudeQ2 = 0,
            AttitudeQ3 = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            IsDeleted = false
        };

        // Add initial state to history
        spacecraft._stateHistory.Add(new SpacecraftState
        {
            Id = Guid.NewGuid(),
            SpacecraftId = spacecraft.Id,
            Epoch = initialEpoch,
            X = x,
            Y = y,
            Z = z,
            Vx = vx,
            Vy = vy,
            Vz = vz,
            FuelMassKg = fuelMassKg,
            CoordinateFrameId = coordinateFrameId,
            RecordedAt = DateTime.UtcNow
        });

        return spacecraft;
    }

    public void Update(
        string? name = null,
        string? description = null,
        double? dryMassKg = null,
        double? dragCoefficient = null,
        double? dragAreaM2 = null,
        double? srpAreaM2 = null,
        double? reflectivityCoefficient = null)
    {
        if (name != null) Name = name;
        if (description != null) Description = description;
        if (dryMassKg.HasValue) DryMassKg = dryMassKg.Value;
        if (dragCoefficient.HasValue) DragCoefficient = dragCoefficient.Value;
        if (dragAreaM2.HasValue) DragAreaM2 = dragAreaM2.Value;
        if (srpAreaM2.HasValue) SrpAreaM2 = srpAreaM2.Value;
        if (reflectivityCoefficient.HasValue) ReflectivityCoefficient = reflectivityCoefficient.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAttitude(AttitudeMode mode, double q0, double q1, double q2, double q3, double spinRate = 0)
    {
        AttitudeMode = mode;
        AttitudeQ0 = q0;
        AttitudeQ1 = q1;
        AttitudeQ2 = q2;
        AttitudeQ3 = q3;
        SpinRateRadPerSec = spinRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMomentOfInertia(double ixx, double iyy, double izz)
    {
        MomentOfInertiaX = ixx;
        MomentOfInertiaY = iyy;
        MomentOfInertiaZ = izz;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConsumeFuel(double amountKg)
    {
        if (amountKg > FuelMassKg)
            throw new InvalidOperationException("Insufficient fuel");

        FuelMassKg -= amountKg;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddState(SpacecraftState state)
    {
        _stateHistory.Add(state);
    }

    public void AddThruster(Thruster thruster)
    {
        _thrusters.Add(thruster);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddFuelTank(FuelTank tank)
    {
        _fuelTanks.Add(tank);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public SpacecraftState? GetStateAtEpoch(DateTime epoch)
    {
        // Find exact match
        var exactMatch = _stateHistory.FirstOrDefault(s => s.Epoch == epoch);
        if (exactMatch != null) return exactMatch;

        // Find bracketing states for interpolation
        var before = _stateHistory.Where(s => s.Epoch < epoch).OrderByDescending(s => s.Epoch).FirstOrDefault();
        var after = _stateHistory.Where(s => s.Epoch > epoch).OrderBy(s => s.Epoch).FirstOrDefault();

        if (before == null && after == null) return null;
        if (before == null) return after;
        if (after == null) return before;

        // Linear interpolation
        var t = (epoch - before.Epoch).TotalSeconds / (after.Epoch - before.Epoch).TotalSeconds;
        return new SpacecraftState
        {
            Id = Guid.NewGuid(),
            SpacecraftId = Id,
            Epoch = epoch,
            X = before.X + t * (after.X - before.X),
            Y = before.Y + t * (after.Y - before.Y),
            Z = before.Z + t * (after.Z - before.Z),
            Vx = before.Vx + t * (after.Vx - before.Vx),
            Vy = before.Vy + t * (after.Vy - before.Vy),
            Vz = before.Vz + t * (after.Vz - before.Vz),
            FuelMassKg = before.FuelMassKg + t * (after.FuelMassKg - before.FuelMassKg),
            CoordinateFrameId = before.CoordinateFrameId,
            RecordedAt = DateTime.UtcNow
        };
    }

    public ValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Mass validation
        if (DryMassKg <= 0) errors.Add("Dry mass must be positive");
        if (FuelMassKg < 0) errors.Add("Fuel mass cannot be negative");
        if (TotalMassKg > 50000) warnings.Add("Total mass exceeds typical spacecraft range");

        // Aerodynamic validation
        if (DragCoefficient < 1 || DragCoefficient > 5) warnings.Add("Drag coefficient typically between 1 and 5");
        if (DragAreaM2 <= 0) errors.Add("Drag area must be positive");

        // SRP validation
        if (SrpAreaM2 <= 0) errors.Add("SRP area must be positive");
        if (ReflectivityCoefficient < 0 || ReflectivityCoefficient > 2)
            warnings.Add("Reflectivity coefficient typically between 0 and 2");

        // State validation
        var r = Math.Sqrt(InitialX * InitialX + InitialY * InitialY + InitialZ * InitialZ);
        if (r < 6378) errors.Add("Initial position is inside Earth");
        if (r > 500000) warnings.Add("Initial position is beyond lunar distance");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }
}

public enum AttitudeMode
{
    NadirPointing,
    SunPointing,
    VelocityPointing,
    Inertial,
    Spinning,
    Custom
}

public sealed class ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
