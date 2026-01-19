namespace Propagation.Core.Entities;

public delegate StateDerivative DerivativeFunction(DateTime epoch, PropagationState state);

public interface IIntegrator
{
    IntegratorType Type { get; }
    (PropagationState newState, double stepTaken, double errorEstimate) Step(
        PropagationState state,
        double requestedStepSize,
        DerivativeFunction derivatives);
}

public sealed class RungeKutta4Integrator : IIntegrator
{
    public IntegratorType Type => IntegratorType.RungeKutta4;

    public (PropagationState newState, double stepTaken, double errorEstimate) Step(
        PropagationState state,
        double requestedStepSize,
        DerivativeFunction derivatives)
    {
        var dt = requestedStepSize;
        var t = state.Epoch;

        // k1 = f(t, y)
        var k1 = derivatives(t, state);

        // k2 = f(t + dt/2, y + dt/2 * k1)
        var state2 = ApplyDerivative(state, k1, dt / 2, t.AddSeconds(dt / 2));
        var k2 = derivatives(t.AddSeconds(dt / 2), state2);

        // k3 = f(t + dt/2, y + dt/2 * k2)
        var state3 = ApplyDerivative(state, k2, dt / 2, t.AddSeconds(dt / 2));
        var k3 = derivatives(t.AddSeconds(dt / 2), state3);

        // k4 = f(t + dt, y + dt * k3)
        var state4 = ApplyDerivative(state, k3, dt, t.AddSeconds(dt));
        var k4 = derivatives(t.AddSeconds(dt), state4);

        // y_new = y + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        var combinedDerivative = (k1 + 2.0 * k2 + 2.0 * k3 + k4) * (1.0 / 6.0);
        var newState = ApplyDerivative(state, combinedDerivative, dt, t.AddSeconds(dt));

        return (newState, dt, 0.0); // RK4 doesn't provide error estimate
    }

    private static PropagationState ApplyDerivative(PropagationState state, StateDerivative d, double dt, DateTime newEpoch)
    {
        return new PropagationState(
            newEpoch,
            state.X + d.Vx * dt,
            state.Y + d.Vy * dt,
            state.Z + d.Vz * dt,
            state.Vx + d.Ax * dt,
            state.Vy + d.Ay * dt,
            state.Vz + d.Az * dt);
    }
}

public sealed class RungeKutta45Integrator : IIntegrator
{
    // Dormand-Prince 5(4) coefficients
    private static readonly double[] C = { 0, 1.0 / 5, 3.0 / 10, 4.0 / 5, 8.0 / 9, 1, 1 };

    private static readonly double[][] A =
    {
        Array.Empty<double>(),
        new[] { 1.0 / 5 },
        new[] { 3.0 / 40, 9.0 / 40 },
        new[] { 44.0 / 45, -56.0 / 15, 32.0 / 9 },
        new[] { 19372.0 / 6561, -25360.0 / 2187, 64448.0 / 6561, -212.0 / 729 },
        new[] { 9017.0 / 3168, -355.0 / 33, 46732.0 / 5247, 49.0 / 176, -5103.0 / 18656 },
        new[] { 35.0 / 384, 0, 500.0 / 1113, 125.0 / 192, -2187.0 / 6784, 11.0 / 84 }
    };

    // 5th order coefficients
    private static readonly double[] B5 = { 35.0 / 384, 0, 500.0 / 1113, 125.0 / 192, -2187.0 / 6784, 11.0 / 84, 0 };

    // 4th order coefficients (for error estimation)
    private static readonly double[] B4 = { 5179.0 / 57600, 0, 7571.0 / 16695, 393.0 / 640, -92097.0 / 339200, 187.0 / 2100, 1.0 / 40 };

    public IntegratorType Type => IntegratorType.RungeKutta45;

    public (PropagationState newState, double stepTaken, double errorEstimate) Step(
        PropagationState state,
        double requestedStepSize,
        DerivativeFunction derivatives)
    {
        var dt = requestedStepSize;
        var t = state.Epoch;
        var k = new StateDerivative[7];

        // Calculate all k values
        k[0] = derivatives(t, state);

        for (int i = 1; i < 7; i++)
        {
            var tNew = t.AddSeconds(C[i] * dt);
            var stateNew = state;

            for (int j = 0; j < i; j++)
            {
                stateNew = ApplyDerivative(stateNew, k[j], A[i][j] * dt, stateNew.Epoch);
            }
            stateNew = new PropagationState(tNew, stateNew.X, stateNew.Y, stateNew.Z, stateNew.Vx, stateNew.Vy, stateNew.Vz);

            k[i] = derivatives(tNew, stateNew);
        }

        // Calculate 5th order solution
        var newState = state;
        for (int i = 0; i < 7; i++)
        {
            newState = ApplyDerivative(newState, k[i], B5[i] * dt, newState.Epoch);
        }
        newState = new PropagationState(t.AddSeconds(dt), newState.X, newState.Y, newState.Z, newState.Vx, newState.Vy, newState.Vz);

        // Calculate 4th order solution for error estimate
        var state4 = state;
        for (int i = 0; i < 7; i++)
        {
            state4 = ApplyDerivative(state4, k[i], B4[i] * dt, state4.Epoch);
        }

        // Error estimate (difference between 5th and 4th order)
        var errorX = Math.Abs(newState.X - state4.X);
        var errorY = Math.Abs(newState.Y - state4.Y);
        var errorZ = Math.Abs(newState.Z - state4.Z);
        var errorVx = Math.Abs(newState.Vx - state4.Vx);
        var errorVy = Math.Abs(newState.Vy - state4.Vy);
        var errorVz = Math.Abs(newState.Vz - state4.Vz);

        var maxError = Math.Max(Math.Max(Math.Max(errorX, errorY), Math.Max(errorZ, errorVx)), Math.Max(errorVy, errorVz));

        return (newState, dt, maxError);
    }

    private static PropagationState ApplyDerivative(PropagationState state, StateDerivative d, double dt, DateTime epoch)
    {
        return new PropagationState(
            epoch,
            state.X + d.Vx * dt,
            state.Y + d.Vy * dt,
            state.Z + d.Vz * dt,
            state.Vx + d.Ax * dt,
            state.Vy + d.Ay * dt,
            state.Vz + d.Az * dt);
    }
}

public static class IntegratorFactory
{
    public static IIntegrator Create(IntegratorType type)
    {
        return type switch
        {
            IntegratorType.RungeKutta4 => new RungeKutta4Integrator(),
            IntegratorType.RungeKutta45 => new RungeKutta45Integrator(),
            IntegratorType.RungeKutta78 => new RungeKutta45Integrator(), // Use RK45 as fallback
            IntegratorType.AdamsBashforth => new RungeKutta45Integrator(), // Use RK45 as fallback
            IntegratorType.GaussJackson => new RungeKutta45Integrator(), // Use RK45 as fallback
            _ => new RungeKutta4Integrator()
        };
    }
}

public static class StepSizeController
{
    private const double SafetyFactor = 0.9;
    private const double MinScaleFactor = 0.1;
    private const double MaxScaleFactor = 5.0;

    public static double ComputeNewStepSize(
        double currentStepSize,
        double error,
        double tolerance,
        int order,
        double minStepSize,
        double maxStepSize)
    {
        if (error < 1e-20)
        {
            // Very small error, increase step size
            return Math.Min(currentStepSize * MaxScaleFactor, maxStepSize);
        }

        // Optimal step size based on error
        var scaleFactor = SafetyFactor * Math.Pow(tolerance / error, 1.0 / (order + 1));

        // Limit scale factor
        scaleFactor = Math.Max(MinScaleFactor, Math.Min(MaxScaleFactor, scaleFactor));

        var newStepSize = currentStepSize * scaleFactor;

        // Apply limits
        return Math.Max(minStepSize, Math.Min(maxStepSize, newStepSize));
    }

    public static bool ShouldRejectStep(double error, double tolerance)
    {
        return error > tolerance;
    }
}
