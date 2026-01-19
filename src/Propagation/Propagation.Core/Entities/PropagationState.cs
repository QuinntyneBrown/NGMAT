namespace Propagation.Core.Entities;

public readonly struct PropagationState
{
    public DateTime Epoch { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }

    public PropagationState(DateTime epoch, double x, double y, double z, double vx, double vy, double vz)
    {
        Epoch = epoch;
        X = x;
        Y = y;
        Z = z;
        Vx = vx;
        Vy = vy;
        Vz = vz;
    }

    public double[] ToArray() => new[] { X, Y, Z, Vx, Vy, Vz };

    public static PropagationState FromArray(DateTime epoch, double[] state)
    {
        if (state.Length != 6)
            throw new ArgumentException("State array must have 6 elements (x, y, z, vx, vy, vz)");

        return new PropagationState(epoch, state[0], state[1], state[2], state[3], state[4], state[5]);
    }

    public double Radius => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
    public double Altitude => Radius - 6378137.0; // Earth equatorial radius

    public static PropagationState operator +(PropagationState a, PropagationState b) =>
        new(a.Epoch, a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.Vx + b.Vx, a.Vy + b.Vy, a.Vz + b.Vz);

    public static PropagationState operator *(PropagationState s, double factor) =>
        new(s.Epoch, s.X * factor, s.Y * factor, s.Z * factor, s.Vx * factor, s.Vy * factor, s.Vz * factor);

    public static PropagationState operator *(double factor, PropagationState s) => s * factor;
}

public readonly struct StateDerivative
{
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public double Ax { get; init; }
    public double Ay { get; init; }
    public double Az { get; init; }

    public StateDerivative(double vx, double vy, double vz, double ax, double ay, double az)
    {
        Vx = vx;
        Vy = vy;
        Vz = vz;
        Ax = ax;
        Ay = ay;
        Az = az;
    }

    public double[] ToArray() => new[] { Vx, Vy, Vz, Ax, Ay, Az };

    public static StateDerivative FromArray(double[] derivatives)
    {
        if (derivatives.Length != 6)
            throw new ArgumentException("Derivative array must have 6 elements");

        return new StateDerivative(
            derivatives[0], derivatives[1], derivatives[2],
            derivatives[3], derivatives[4], derivatives[5]);
    }

    public static StateDerivative operator +(StateDerivative a, StateDerivative b) =>
        new(a.Vx + b.Vx, a.Vy + b.Vy, a.Vz + b.Vz, a.Ax + b.Ax, a.Ay + b.Ay, a.Az + b.Az);

    public static StateDerivative operator *(StateDerivative d, double factor) =>
        new(d.Vx * factor, d.Vy * factor, d.Vz * factor, d.Ax * factor, d.Ay * factor, d.Az * factor);

    public static StateDerivative operator *(double factor, StateDerivative d) => d * factor;

    public static StateDerivative Zero => new(0, 0, 0, 0, 0, 0);
}

public sealed class PropagationResult
{
    public Guid Id { get; private set; }
    public Guid? PropagationConfigurationId { get; private set; }
    public Guid? SpacecraftId { get; private set; }
    public DateTime StartEpoch { get; private set; }
    public DateTime EndEpoch { get; private set; }
    public List<PropagationState> States { get; private set; } = new();
    public int StepCount { get; private set; }
    public double ComputationTimeMs { get; private set; }
    public bool WasSuccessful { get; private set; }
    public string? ErrorMessage { get; private set; }
    public PropagationTerminationReason TerminationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PropagationResult() { }

    public static PropagationResult CreateSuccessful(
        DateTime startEpoch,
        DateTime endEpoch,
        List<PropagationState> states,
        int stepCount,
        double computationTimeMs,
        PropagationTerminationReason terminationReason,
        Guid? propagationConfigurationId = null,
        Guid? spacecraftId = null)
    {
        return new PropagationResult
        {
            Id = Guid.NewGuid(),
            PropagationConfigurationId = propagationConfigurationId,
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            States = states,
            StepCount = stepCount,
            ComputationTimeMs = computationTimeMs,
            WasSuccessful = true,
            TerminationReason = terminationReason,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PropagationResult CreateFailed(
        DateTime startEpoch,
        string errorMessage,
        PropagationTerminationReason terminationReason,
        Guid? propagationConfigurationId = null,
        Guid? spacecraftId = null)
    {
        return new PropagationResult
        {
            Id = Guid.NewGuid(),
            PropagationConfigurationId = propagationConfigurationId,
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = startEpoch,
            WasSuccessful = false,
            ErrorMessage = errorMessage,
            TerminationReason = terminationReason,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum PropagationTerminationReason
{
    ReachedEndEpoch,
    ReachedMaxSteps,
    BelowMinAltitude,
    IntegrationError,
    UserCancelled,
    Unknown
}
