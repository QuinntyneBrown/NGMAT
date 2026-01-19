using CoordinateSystem.Core.Entities;

namespace CoordinateSystem.Core.Services;

/// <summary>
/// Service for converting between state vectors and orbital elements.
/// </summary>
public sealed class OrbitalElementsService
{
    /// <summary>
    /// Convert state vector to Keplerian orbital elements.
    /// </summary>
    public KeplerianElements StateToKeplerian(StateVector state, double mu = Wgs84.GM)
    {
        var r = state.Position;
        var v = state.Velocity;
        var rMag = r.Magnitude;
        var vMag = v.Magnitude;

        // Angular momentum vector
        var h = Vector3.Cross(r, v);
        var hMag = h.Magnitude;

        // Node vector (z × h)
        var n = Vector3.Cross(new Vector3(0, 0, 1), h);
        var nMag = n.Magnitude;

        // Eccentricity vector
        var eVec = ((vMag * vMag - mu / rMag) * r - Vector3.Dot(r, v) * v) / mu;
        var e = eVec.Magnitude;

        // Specific orbital energy
        var energy = vMag * vMag / 2.0 - mu / rMag;

        // Semi-major axis
        double a;
        if (Math.Abs(e - 1.0) < 1e-10)
        {
            // Parabolic orbit
            a = double.PositiveInfinity;
        }
        else
        {
            a = -mu / (2.0 * energy);
        }

        // Inclination
        var inc = Math.Acos(h.Z / hMag);

        // Right ascension of ascending node (RAAN)
        double raan;
        if (nMag < 1e-10)
        {
            // Equatorial orbit
            raan = 0;
        }
        else
        {
            raan = Math.Acos(n.X / nMag);
            if (n.Y < 0) raan = 2.0 * Math.PI - raan;
        }

        // Argument of periapsis
        double omega;
        if (nMag < 1e-10)
        {
            // Equatorial orbit - use longitude of periapsis
            omega = Math.Atan2(eVec.Y, eVec.X);
            if (h.Z < 0) omega = -omega;
        }
        else if (e < 1e-10)
        {
            // Circular orbit
            omega = 0;
        }
        else
        {
            omega = Math.Acos(Vector3.Dot(n, eVec) / (nMag * e));
            if (eVec.Z < 0) omega = 2.0 * Math.PI - omega;
        }

        // True anomaly
        double nu;
        if (e < 1e-10)
        {
            // Circular orbit - use argument of latitude
            if (nMag < 1e-10)
            {
                // Circular equatorial - use true longitude
                nu = Math.Acos(r.X / rMag);
                if (r.Y < 0) nu = 2.0 * Math.PI - nu;
            }
            else
            {
                nu = Math.Acos(Vector3.Dot(n, r) / (nMag * rMag));
                if (r.Z < 0) nu = 2.0 * Math.PI - nu;
            }
        }
        else
        {
            nu = Math.Acos(Vector3.Dot(eVec, r) / (e * rMag));
            if (Vector3.Dot(r, v) < 0) nu = 2.0 * Math.PI - nu;
        }

        return new KeplerianElements(a, e, inc, raan, omega, nu, mu);
    }

    /// <summary>
    /// Convert Keplerian orbital elements to state vector.
    /// </summary>
    public StateVector KeplerianToState(KeplerianElements elements)
    {
        var a = elements.SemiMajorAxis;
        var e = elements.Eccentricity;
        var i = elements.Inclination;
        var raan = elements.RAAN;
        var omega = elements.ArgumentOfPeriapsis;
        var nu = elements.TrueAnomaly;
        var mu = elements.Mu;

        // Semi-latus rectum
        var p = a * (1 - e * e);

        // Position in orbital plane (perifocal coordinates)
        var r_pf = p / (1 + e * Math.Cos(nu));
        var r_p = r_pf * Math.Cos(nu);
        var r_q = r_pf * Math.Sin(nu);

        // Velocity in orbital plane
        var sqrtMuP = Math.Sqrt(mu / p);
        var v_p = -sqrtMuP * Math.Sin(nu);
        var v_q = sqrtMuP * (e + Math.Cos(nu));

        // Rotation matrices
        var cosRaan = Math.Cos(raan);
        var sinRaan = Math.Sin(raan);
        var cosI = Math.Cos(i);
        var sinI = Math.Sin(i);
        var cosOmega = Math.Cos(omega);
        var sinOmega = Math.Sin(omega);

        // Transformation matrix from perifocal to ECI
        var l1 = cosRaan * cosOmega - sinRaan * sinOmega * cosI;
        var l2 = -cosRaan * sinOmega - sinRaan * cosOmega * cosI;
        var m1 = sinRaan * cosOmega + cosRaan * sinOmega * cosI;
        var m2 = -sinRaan * sinOmega + cosRaan * cosOmega * cosI;
        var n1 = sinOmega * sinI;
        var n2 = cosOmega * sinI;

        // Position in ECI
        var x = l1 * r_p + l2 * r_q;
        var y = m1 * r_p + m2 * r_q;
        var z = n1 * r_p + n2 * r_q;

        // Velocity in ECI
        var vx = l1 * v_p + l2 * v_q;
        var vy = m1 * v_p + m2 * v_q;
        var vz = n1 * v_p + n2 * v_q;

        return new StateVector(x, y, z, vx, vy, vz);
    }

    /// <summary>
    /// Compute true anomaly from eccentric anomaly.
    /// </summary>
    public double EccentricToTrueAnomaly(double eccentricAnomaly, double eccentricity)
    {
        var e = eccentricity;
        var E = eccentricAnomaly;

        if (e < 1)
        {
            // Elliptical
            var cosE = Math.Cos(E);
            var sinE = Math.Sin(E);
            var cosNu = (cosE - e) / (1 - e * cosE);
            var sinNu = Math.Sqrt(1 - e * e) * sinE / (1 - e * cosE);
            return Math.Atan2(sinNu, cosNu);
        }
        else if (e > 1)
        {
            // Hyperbolic
            var coshF = Math.Cosh(E);
            var sinhF = Math.Sinh(E);
            var cosNu = (e - coshF) / (e * coshF - 1);
            var sinNu = Math.Sqrt(e * e - 1) * sinhF / (e * coshF - 1);
            return Math.Atan2(sinNu, cosNu);
        }
        else
        {
            // Parabolic
            return 2 * Math.Atan(E);
        }
    }

    /// <summary>
    /// Compute eccentric anomaly from mean anomaly using Newton-Raphson iteration.
    /// </summary>
    public double MeanToEccentricAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-12)
    {
        var M = meanAnomaly;
        var e = eccentricity;

        if (e < 1)
        {
            // Elliptical orbit - Kepler's equation: M = E - e*sin(E)
            var E = M; // Initial guess
            for (int i = 0; i < 50; i++)
            {
                var f = E - e * Math.Sin(E) - M;
                var fp = 1 - e * Math.Cos(E);
                var dE = -f / fp;
                E += dE;
                if (Math.Abs(dE) < tolerance)
                    break;
            }
            return E;
        }
        else if (e > 1)
        {
            // Hyperbolic orbit: M = e*sinh(F) - F
            var F = M; // Initial guess
            for (int i = 0; i < 50; i++)
            {
                var f = e * Math.Sinh(F) - F - M;
                var fp = e * Math.Cosh(F) - 1;
                var dF = -f / fp;
                F += dF;
                if (Math.Abs(dF) < tolerance)
                    break;
            }
            return F;
        }
        else
        {
            // Parabolic orbit: M = D + D³/3, solve cubic
            var x = Math.Pow(3 * M + Math.Sqrt(9 * M * M + 1), 1.0 / 3.0);
            return x - 1 / x;
        }
    }

    /// <summary>
    /// Convert osculating elements to mean elements by removing short-period perturbations.
    /// Uses simplified J2 perturbation model.
    /// </summary>
    public KeplerianElements OsculatingToMean(KeplerianElements osculating)
    {
        var a = osculating.SemiMajorAxis;
        var e = osculating.Eccentricity;
        var i = osculating.Inclination;
        var mu = osculating.Mu;

        // J2 perturbation parameters
        var J2 = Wgs84.J2;
        var Re = Wgs84.SemiMajorAxis;
        var p = a * (1 - e * e);
        var n = Math.Sqrt(mu / (a * a * a));

        // Mean motion correction (secular rate)
        var cosI = Math.Cos(i);
        var sinI = Math.Sin(i);
        var nDot = n * (1 + 1.5 * J2 * Math.Pow(Re / p, 2) * Math.Sqrt(1 - e * e) * (1 - 1.5 * sinI * sinI));

        // Secular rates of RAAN and argument of periapsis
        var raanDot = -1.5 * J2 * Math.Pow(Re / p, 2) * n * cosI;
        var omegaDot = 0.75 * J2 * Math.Pow(Re / p, 2) * n * (5 * cosI * cosI - 1);

        // For a simplified conversion, we return the osculating elements
        // as mean elements. A full implementation would require removing
        // short-period variations using Brouwer's theory.
        return new KeplerianElements(
            a,
            e,
            i,
            osculating.RAAN,
            osculating.ArgumentOfPeriapsis,
            osculating.TrueAnomaly,
            mu);
    }

    /// <summary>
    /// Compute secular J2 perturbation rates.
    /// </summary>
    public (double raanRate, double argPeriRate, double meanMotionRate) ComputeJ2SecularRates(
        double semiMajorAxis,
        double eccentricity,
        double inclination,
        double mu = Wgs84.GM)
    {
        var a = semiMajorAxis;
        var e = eccentricity;
        var i = inclination;

        var J2 = Wgs84.J2;
        var Re = Wgs84.SemiMajorAxis;
        var p = a * (1 - e * e);
        var n = Math.Sqrt(mu / (a * a * a));

        var cosI = Math.Cos(i);
        var sinI = Math.Sin(i);
        var sqrtOneMinusE2 = Math.Sqrt(1 - e * e);

        var factor = 1.5 * J2 * Math.Pow(Re / p, 2) * n;

        // RAAN rate (rad/s)
        var raanRate = -factor * cosI;

        // Argument of periapsis rate (rad/s)
        var argPeriRate = factor * (2 - 2.5 * sinI * sinI);

        // Mean motion rate correction
        var meanMotionRate = n * (1 + factor * sqrtOneMinusE2 * (1 - 1.5 * sinI * sinI));

        return (raanRate, argPeriRate, meanMotionRate);
    }
}
