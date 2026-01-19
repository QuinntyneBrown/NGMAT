namespace ForceModel.Core.Entities;

public static class GravityConstants
{
    // Earth gravitational constant (EGM2008)
    public const double EarthGM = 3.986004418e14; // m^3/s^2

    // Earth equatorial radius (WGS-84)
    public const double EarthRadius = 6378137.0; // m

    // Earth J2 coefficient (EGM2008)
    public const double EarthJ2 = 1.08262668e-3;

    // Earth J3 coefficient (EGM2008)
    public const double EarthJ3 = -2.53265649e-6;

    // Earth J4 coefficient (EGM2008)
    public const double EarthJ4 = -1.61962159e-6;

    // Earth J5 coefficient (EGM2008)
    public const double EarthJ5 = -2.27296083e-7;

    // Earth J6 coefficient (EGM2008)
    public const double EarthJ6 = 5.40681239e-7;

    // Moon gravitational constant
    public const double MoonGM = 4.9028000661e12; // m^3/s^2

    // Sun gravitational constant
    public const double SunGM = 1.32712440041279419e20; // m^3/s^2

    // Speed of light
    public const double SpeedOfLight = 299792458.0; // m/s
}

public readonly struct GravityAcceleration
{
    public double Ax { get; init; }
    public double Ay { get; init; }
    public double Az { get; init; }

    public GravityAcceleration(double ax, double ay, double az)
    {
        Ax = ax;
        Ay = ay;
        Az = az;
    }

    public double Magnitude => Math.Sqrt(Ax * Ax + Ay * Ay + Az * Az);

    public static GravityAcceleration operator +(GravityAcceleration a, GravityAcceleration b) =>
        new(a.Ax + b.Ax, a.Ay + b.Ay, a.Az + b.Az);

    public static GravityAcceleration Zero => new(0, 0, 0);
}

public readonly struct SpacecraftState
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double Vz { get; init; }
    public DateTime Epoch { get; init; }

    public SpacecraftState(double x, double y, double z, double vx, double vy, double vz, DateTime epoch)
    {
        X = x;
        Y = y;
        Z = z;
        Vx = vx;
        Vy = vy;
        Vz = vz;
        Epoch = epoch;
    }

    public double Radius => Math.Sqrt(X * X + Y * Y + Z * Z);
    public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
    public double Altitude => Radius - GravityConstants.EarthRadius;
}

public readonly struct SpacecraftProperties
{
    public double MassKg { get; init; }
    public double DragCoefficient { get; init; }
    public double DragAreaM2 { get; init; }
    public double SrpAreaM2 { get; init; }
    public double ReflectivityCoefficient { get; init; }

    public SpacecraftProperties(
        double massKg,
        double dragCoefficient,
        double dragAreaM2,
        double srpAreaM2,
        double reflectivityCoefficient)
    {
        MassKg = massKg;
        DragCoefficient = dragCoefficient;
        DragAreaM2 = dragAreaM2;
        SrpAreaM2 = srpAreaM2;
        ReflectivityCoefficient = reflectivityCoefficient;
    }

    public double BallisticCoefficient => MassKg / (DragCoefficient * DragAreaM2);
    public double SrpCoefficient => SrpAreaM2 * (1 + ReflectivityCoefficient) / MassKg;
}

public static class PointMassGravity
{
    public static GravityAcceleration Calculate(SpacecraftState state, double gm = GravityConstants.EarthGM)
    {
        var r = state.Radius;
        var r3 = r * r * r;
        var factor = -gm / r3;

        return new GravityAcceleration(
            factor * state.X,
            factor * state.Y,
            factor * state.Z);
    }
}

public static class J2Gravity
{
    public static GravityAcceleration Calculate(
        SpacecraftState state,
        double gm = GravityConstants.EarthGM,
        double re = GravityConstants.EarthRadius,
        double j2 = GravityConstants.EarthJ2)
    {
        var x = state.X;
        var y = state.Y;
        var z = state.Z;
        var r = state.Radius;
        var r2 = r * r;
        var r5 = r2 * r2 * r;

        var z2 = z * z;
        var re2 = re * re;

        // J2 perturbation components
        var j2Factor = 1.5 * j2 * gm * re2 / r5;
        var zFactor = 5.0 * z2 / r2;

        var ax = -gm * x / (r2 * r) + j2Factor * x * (zFactor - 1.0);
        var ay = -gm * y / (r2 * r) + j2Factor * y * (zFactor - 1.0);
        var az = -gm * z / (r2 * r) + j2Factor * z * (zFactor - 3.0);

        return new GravityAcceleration(ax, ay, az);
    }
}

public static class J2J3Gravity
{
    public static GravityAcceleration Calculate(
        SpacecraftState state,
        double gm = GravityConstants.EarthGM,
        double re = GravityConstants.EarthRadius,
        double j2 = GravityConstants.EarthJ2,
        double j3 = GravityConstants.EarthJ3)
    {
        var x = state.X;
        var y = state.Y;
        var z = state.Z;
        var r = state.Radius;
        var r2 = r * r;
        var r3 = r2 * r;
        var r5 = r2 * r3;
        var r7 = r5 * r2;

        var z2 = z * z;
        var re2 = re * re;
        var re3 = re2 * re;

        // Point mass
        var pmFactor = -gm / r3;

        // J2 perturbation
        var j2Factor = 1.5 * j2 * gm * re2 / r5;
        var zFactor2 = 5.0 * z2 / r2;

        // J3 perturbation
        var j3Factor = 2.5 * j3 * gm * re3 / r7;
        var zFactor3 = 7.0 * z2 / r2;

        var ax = pmFactor * x + j2Factor * x * (zFactor2 - 1.0) + j3Factor * x * z * (zFactor3 - 3.0);
        var ay = pmFactor * y + j2Factor * y * (zFactor2 - 1.0) + j3Factor * y * z * (zFactor3 - 3.0);
        var az = pmFactor * z + j2Factor * z * (zFactor2 - 3.0) + j3Factor * (z2 * (zFactor3 - 6.0) + 3.0 * r2 / 5.0);

        return new GravityAcceleration(ax, ay, az);
    }
}

public static class ThirdBodyGravity
{
    public static GravityAcceleration Calculate(
        SpacecraftState state,
        double bodyX, double bodyY, double bodyZ,
        double bodyGM)
    {
        // Vector from Earth center to third body
        // Vector from spacecraft to third body
        var sx = bodyX - state.X;
        var sy = bodyY - state.Y;
        var sz = bodyZ - state.Z;

        var sNorm = Math.Sqrt(sx * sx + sy * sy + sz * sz);
        var bNorm = Math.Sqrt(bodyX * bodyX + bodyY * bodyY + bodyZ * bodyZ);

        var s3 = sNorm * sNorm * sNorm;
        var b3 = bNorm * bNorm * bNorm;

        // Acceleration: GM * (s/|s|^3 - b/|b|^3)
        var ax = bodyGM * (sx / s3 - bodyX / b3);
        var ay = bodyGM * (sy / s3 - bodyY / b3);
        var az = bodyGM * (sz / s3 - bodyZ / b3);

        return new GravityAcceleration(ax, ay, az);
    }
}
