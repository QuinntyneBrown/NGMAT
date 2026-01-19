namespace ForceModel.Core.Entities;

public readonly struct AtmosphericDensity
{
    public double DensityKgM3 { get; init; }
    public double TemperatureK { get; init; }
    public double ScaleHeightM { get; init; }

    public AtmosphericDensity(double density, double temperature = 0, double scaleHeight = 0)
    {
        DensityKgM3 = density;
        TemperatureK = temperature;
        ScaleHeightM = scaleHeight;
    }
}

public static class ExponentialAtmosphere
{
    // Reference density at sea level (kg/m^3)
    private const double Rho0 = 1.225;

    // Scale height (m)
    private const double H0 = 8500.0;

    // Reference altitude (m)
    private const double H_Ref = 0.0;

    // Altitude-density table for more accurate model
    private static readonly (double altitude, double density, double scaleHeight)[] DensityTable =
    {
        (0, 1.225, 7249),
        (25000, 3.899e-2, 6349),
        (30000, 1.774e-2, 6682),
        (40000, 3.972e-3, 7554),
        (50000, 1.057e-3, 8382),
        (60000, 3.206e-4, 7714),
        (70000, 8.770e-5, 6549),
        (80000, 1.905e-5, 5799),
        (90000, 3.396e-6, 5382),
        (100000, 5.604e-7, 5877),
        (110000, 9.708e-8, 7263),
        (120000, 2.222e-8, 9473),
        (130000, 8.152e-9, 12636),
        (140000, 3.831e-9, 16149),
        (150000, 2.076e-9, 22523),
        (180000, 5.194e-10, 29740),
        (200000, 2.541e-10, 37105),
        (250000, 6.073e-11, 45546),
        (300000, 1.916e-11, 53628),
        (350000, 7.014e-12, 53298),
        (400000, 2.803e-12, 58515),
        (450000, 1.184e-12, 60828),
        (500000, 5.215e-13, 63822),
        (600000, 1.137e-13, 71835),
        (700000, 3.070e-14, 88667),
        (800000, 1.136e-14, 124640),
        (900000, 5.759e-15, 181050),
        (1000000, 3.561e-15, 268000)
    };

    public static AtmosphericDensity Calculate(double altitudeM)
    {
        if (altitudeM < 0)
        {
            return new AtmosphericDensity(Rho0, 288.15, H0);
        }

        // Find bracketing entries
        var lower = DensityTable[0];
        var upper = DensityTable[^1];

        for (int i = 0; i < DensityTable.Length - 1; i++)
        {
            if (altitudeM >= DensityTable[i].altitude && altitudeM < DensityTable[i + 1].altitude)
            {
                lower = DensityTable[i];
                upper = DensityTable[i + 1];
                break;
            }
        }

        if (altitudeM >= DensityTable[^1].altitude)
        {
            // Above table, use exponential decay
            var (h0, rho0, scaleH) = DensityTable[^1];
            var density = rho0 * Math.Exp(-(altitudeM - h0) / scaleH);
            return new AtmosphericDensity(density, 1000, scaleH);
        }

        // Exponential interpolation
        var h = altitudeM - lower.altitude;
        var scale = lower.scaleHeight;
        var rho = lower.density * Math.Exp(-h / scale);

        return new AtmosphericDensity(rho, 0, scale);
    }

    public static AtmosphericDensity CalculateSimple(double altitudeM)
    {
        var density = Rho0 * Math.Exp(-(altitudeM - H_Ref) / H0);
        return new AtmosphericDensity(density, 288.15, H0);
    }
}

public static class HarrisPriesterAtmosphere
{
    // Harris-Priester density tables for minimum and maximum solar activity
    private static readonly (double altitude, double rhoMin, double rhoMax)[] DensityTable =
    {
        (100000, 4.974e-7, 4.974e-7),
        (120000, 2.490e-8, 2.490e-8),
        (130000, 8.377e-9, 8.710e-9),
        (140000, 3.899e-9, 4.059e-9),
        (150000, 2.122e-9, 2.215e-9),
        (160000, 1.263e-9, 1.344e-9),
        (170000, 8.008e-10, 8.758e-10),
        (180000, 5.283e-10, 6.010e-10),
        (190000, 3.617e-10, 4.297e-10),
        (200000, 2.557e-10, 3.162e-10),
        (210000, 1.839e-10, 2.396e-10),
        (220000, 1.341e-10, 1.853e-10),
        (230000, 9.949e-11, 1.455e-10),
        (240000, 7.488e-11, 1.157e-10),
        (250000, 5.709e-11, 9.308e-11),
        (260000, 4.403e-11, 7.555e-11),
        (270000, 3.430e-11, 6.182e-11),
        (280000, 2.697e-11, 5.095e-11),
        (290000, 2.139e-11, 4.226e-11),
        (300000, 1.708e-11, 3.526e-11),
        (320000, 1.099e-11, 2.511e-11),
        (340000, 7.214e-12, 1.819e-11),
        (360000, 4.824e-12, 1.337e-11),
        (380000, 3.274e-12, 9.955e-12),
        (400000, 2.249e-12, 7.492e-12),
        (420000, 1.558e-12, 5.684e-12),
        (440000, 1.091e-12, 4.355e-12),
        (460000, 7.701e-13, 3.362e-12),
        (480000, 5.474e-13, 2.612e-12),
        (500000, 3.916e-13, 2.042e-12),
        (600000, 8.995e-14, 5.215e-13),
        (700000, 2.418e-14, 1.497e-13),
        (800000, 7.248e-15, 4.806e-14),
        (900000, 2.418e-15, 1.695e-14),
        (1000000, 8.996e-16, 6.509e-15)
    };

    // Exponent for lag angle calculation
    private const double N = 2.0;

    public static AtmosphericDensity Calculate(
        double altitudeM,
        double sunRightAscensionRad,
        double spacecraftRightAscensionRad,
        double spacecraftDeclinationRad,
        double f107)
    {
        if (altitudeM < 100000 || altitudeM > 1000000)
        {
            return ExponentialAtmosphere.Calculate(altitudeM);
        }

        // Find bracketing entries
        var lower = DensityTable[0];
        var upper = DensityTable[^1];

        for (int i = 0; i < DensityTable.Length - 1; i++)
        {
            if (altitudeM >= DensityTable[i].altitude && altitudeM < DensityTable[i + 1].altitude)
            {
                lower = DensityTable[i];
                upper = DensityTable[i + 1];
                break;
            }
        }

        // Interpolate min and max densities
        var frac = (altitudeM - lower.altitude) / (upper.altitude - lower.altitude);
        var rhoMin = lower.rhoMin * Math.Pow(upper.rhoMin / lower.rhoMin, frac);
        var rhoMax = lower.rhoMax * Math.Pow(upper.rhoMax / lower.rhoMax, frac);

        // Calculate diurnal variation based on spacecraft position relative to Sun
        // Lag angle of density bulge behind subsolar point (about 30 degrees)
        const double lagAngle = 0.523599; // 30 degrees in radians

        // Hour angle from Sun
        var hourAngle = spacecraftRightAscensionRad - sunRightAscensionRad - lagAngle;
        var cosHalfPsi = Math.Cos(hourAngle / 2.0) * Math.Cos(spacecraftDeclinationRad / 2.0);

        // Diurnal factor
        var diurnalFactor = Math.Pow(cosHalfPsi, N);
        if (cosHalfPsi < 0) diurnalFactor = 0;

        // Interpolate based on F10.7 solar flux
        // F10.7 ranges from ~70 (solar minimum) to ~250 (solar maximum)
        var solarFactor = (f107 - 70.0) / 180.0;
        solarFactor = Math.Max(0, Math.Min(1, solarFactor));

        var rhoNight = rhoMin + solarFactor * (rhoMax - rhoMin) * 0.3;
        var rhoDay = rhoMin + solarFactor * (rhoMax - rhoMin);

        var density = rhoNight + (rhoDay - rhoNight) * diurnalFactor;

        return new AtmosphericDensity(density);
    }
}

public static class AtmosphericDrag
{
    public static GravityAcceleration Calculate(
        SpacecraftState state,
        SpacecraftProperties props,
        AtmosphericDensity atmosphere)
    {
        if (atmosphere.DensityKgM3 <= 0)
        {
            return GravityAcceleration.Zero;
        }

        // Velocity relative to atmosphere (assuming rotating Earth)
        // Earth rotation rate: 7.2921159e-5 rad/s
        const double omegaEarth = 7.2921159e-5;

        var vRelX = state.Vx + omegaEarth * state.Y;
        var vRelY = state.Vy - omegaEarth * state.X;
        var vRelZ = state.Vz;

        var vRel = Math.Sqrt(vRelX * vRelX + vRelY * vRelY + vRelZ * vRelZ);

        if (vRel < 1e-6)
        {
            return GravityAcceleration.Zero;
        }

        // Drag acceleration: a = -0.5 * rho * Cd * A * v^2 / m * v_hat
        var factor = -0.5 * atmosphere.DensityKgM3 * props.DragCoefficient * props.DragAreaM2 * vRel / props.MassKg;

        return new GravityAcceleration(
            factor * vRelX,
            factor * vRelY,
            factor * vRelZ);
    }
}
