using Shared.Domain.Results;
using Visualization.Core.Models;

namespace Visualization.Core.Services;

public sealed class VisualizationService
{
    private const double EarthRadiusKm = 6378.137;
    private const double EarthGM = 398600.4418;  // km^3/s^2

    /// <summary>
    /// Generate 3D orbit plot data from state vectors
    /// </summary>
    public Result<OrbitPlotData> GenerateOrbitPlot(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null,
        string centralBody = "Earth")
    {
        if (endEpoch <= startEpoch)
        {
            return Result<OrbitPlotData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<OrbitPoint3D>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };  // Default LEO

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            var x = propagatedState[0];
            var y = propagatedState[1];
            var z = propagatedState[2];
            var vx = propagatedState[3];
            var vy = propagatedState[4];
            var vz = propagatedState[5];

            var r = Math.Sqrt(x * x + y * y + z * z);
            var v = Math.Sqrt(vx * vx + vy * vy + vz * vz);

            points.Add(new OrbitPoint3D
            {
                Epoch = currentEpoch,
                X = x,
                Y = y,
                Z = z,
                Vx = vx,
                Vy = vy,
                Vz = vz,
                Altitude = r - EarthRadiusKm,
                Speed = v
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var elements = StateToElements(state);
        var metadata = new OrbitMetadata
        {
            SemiMajorAxisKm = elements.a,
            Eccentricity = elements.e,
            InclinationDeg = elements.i * 180.0 / Math.PI,
            RaanDeg = elements.raan * 180.0 / Math.PI,
            ArgPeriapsisDeg = elements.omega * 180.0 / Math.PI,
            PeriodMinutes = 2 * Math.PI * Math.Sqrt(Math.Pow(elements.a, 3) / EarthGM) / 60.0,
            ApogeeKm = elements.a * (1 + elements.e) - EarthRadiusKm,
            PerigeeKm = elements.a * (1 - elements.e) - EarthRadiusKm
        };

        var result = new OrbitPlotData
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            PointCount = points.Count,
            Points = points,
            CentralBody = new CentralBodyData
            {
                Name = centralBody,
                RadiusKm = EarthRadiusKm
            },
            Metadata = metadata
        };

        return Result<OrbitPlotData>.Success(result);
    }

    /// <summary>
    /// Generate ground track (lat/lon) data
    /// </summary>
    public Result<GroundTrackData> GenerateGroundTrack(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<GroundTrackData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<GroundTrackPoint>();
        var segments = new List<GroundTrackSegment>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        double? prevLongitude = null;
        int segmentStart = 0;
        bool? prevAscending = null;

        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            // Convert ECI to geodetic
            var (lat, lon, alt) = EciToGeodetic(propagatedState, currentEpoch);
            var isAscending = propagatedState[5] > 0;  // Positive z-velocity = ascending

            // Check for antimeridian crossing
            if (prevLongitude.HasValue)
            {
                var lonDiff = Math.Abs(lon - prevLongitude.Value);
                if (lonDiff > 180)
                {
                    // Segment ends
                    segments.Add(new GroundTrackSegment
                    {
                        StartIndex = segmentStart,
                        EndIndex = points.Count - 1,
                        IsAscending = prevAscending ?? isAscending,
                        CrossesAntimeridian = true
                    });
                    segmentStart = points.Count;
                }
            }

            // Check for ascending/descending change
            if (prevAscending.HasValue && prevAscending.Value != isAscending)
            {
                segments.Add(new GroundTrackSegment
                {
                    StartIndex = segmentStart,
                    EndIndex = points.Count - 1,
                    IsAscending = prevAscending.Value,
                    CrossesAntimeridian = false
                });
                segmentStart = points.Count;
            }

            points.Add(new GroundTrackPoint
            {
                Epoch = currentEpoch,
                LatitudeDeg = lat,
                LongitudeDeg = lon,
                AltitudeKm = alt,
                IsAscending = isAscending,
                IsDaylit = IsDaylit(propagatedState, currentEpoch)
            });

            prevLongitude = lon;
            prevAscending = isAscending;
            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        // Add final segment
        if (points.Count > segmentStart)
        {
            segments.Add(new GroundTrackSegment
            {
                StartIndex = segmentStart,
                EndIndex = points.Count - 1,
                IsAscending = prevAscending ?? true,
                CrossesAntimeridian = false
            });
        }

        var result = new GroundTrackData
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            PointCount = points.Count,
            Points = points,
            Segments = segments
        };

        return Result<GroundTrackData>.Success(result);
    }

    /// <summary>
    /// Generate time-series data for a parameter
    /// </summary>
    public Result<TimeSeriesData> GenerateTimeSeries(
        string parameterName,
        Guid? spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<TimeSeriesData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<TimeSeriesPoint>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            var value = parameterName.ToLower() switch
            {
                "altitude" => Math.Sqrt(propagatedState[0] * propagatedState[0] +
                                       propagatedState[1] * propagatedState[1] +
                                       propagatedState[2] * propagatedState[2]) - EarthRadiusKm,
                "velocity" or "speed" => Math.Sqrt(propagatedState[3] * propagatedState[3] +
                                                   propagatedState[4] * propagatedState[4] +
                                                   propagatedState[5] * propagatedState[5]),
                "x" => propagatedState[0],
                "y" => propagatedState[1],
                "z" => propagatedState[2],
                "vx" => propagatedState[3],
                "vy" => propagatedState[4],
                "vz" => propagatedState[5],
                _ => 0
            };

            points.Add(new TimeSeriesPoint
            {
                Epoch = currentEpoch,
                Value = value
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var values = points.Select(p => p.Value).ToList();
        var stats = new TimeSeriesStatistics
        {
            Min = values.Min(),
            Max = values.Max(),
            Mean = values.Average(),
            StandardDeviation = CalculateStdDev(values)
        };

        var unit = parameterName.ToLower() switch
        {
            "altitude" => "km",
            "velocity" or "speed" => "km/s",
            "x" or "y" or "z" => "km",
            "vx" or "vy" or "vz" => "km/s",
            _ => ""
        };

        var result = new TimeSeriesData
        {
            SpacecraftId = spacecraftId,
            ParameterName = parameterName,
            Unit = unit,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            PointCount = points.Count,
            Points = points,
            Statistics = stats
        };

        return Result<TimeSeriesData>.Success(result);
    }

    /// <summary>
    /// Generate orbital elements over time
    /// </summary>
    public Result<OrbitalElementsData> GenerateOrbitalElements(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<OrbitalElementsData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<OrbitalElementsPoint>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);
            var elements = StateToElements(propagatedState);

            points.Add(new OrbitalElementsPoint
            {
                Epoch = currentEpoch,
                SemiMajorAxisKm = elements.a,
                Eccentricity = elements.e,
                InclinationDeg = elements.i * 180.0 / Math.PI,
                RaanDeg = elements.raan * 180.0 / Math.PI,
                ArgPeriapsisDeg = elements.omega * 180.0 / Math.PI,
                TrueAnomalyDeg = elements.nu * 180.0 / Math.PI,
                MeanAnomalyDeg = TrueToMeanAnomaly(elements.nu, elements.e) * 180.0 / Math.PI
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var result = new OrbitalElementsData
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            PointCount = points.Count,
            Points = points
        };

        return Result<OrbitalElementsData>.Success(result);
    }

    /// <summary>
    /// Generate eclipse data
    /// </summary>
    public Result<EclipseData> GenerateEclipseData(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<EclipseData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<EclipsePoint>();
        var events = new List<EclipseEvent>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        bool wasInUmbra = false;
        bool wasInPenumbra = false;
        DateTime? umbraEntry = null;
        DateTime? penumbraEntry = null;
        var totalUmbra = TimeSpan.Zero;
        var totalPenumbra = TimeSpan.Zero;
        var maxEclipseDuration = TimeSpan.Zero;

        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            var (inUmbra, inPenumbra, sunVisibility, sunAngle) = CalculateEclipse(propagatedState, currentEpoch);

            points.Add(new EclipsePoint
            {
                Epoch = currentEpoch,
                SunVisibilityPercent = sunVisibility,
                InUmbra = inUmbra,
                InPenumbra = inPenumbra,
                SunAngleDeg = sunAngle
            });

            // Track umbra events
            if (inUmbra && !wasInUmbra)
            {
                umbraEntry = currentEpoch;
            }
            else if (!inUmbra && wasInUmbra && umbraEntry.HasValue)
            {
                var duration = currentEpoch - umbraEntry.Value;
                events.Add(new EclipseEvent
                {
                    Type = EclipseType.Umbra,
                    EntryTime = umbraEntry.Value,
                    ExitTime = currentEpoch
                });
                totalUmbra += duration;
                if (duration > maxEclipseDuration) maxEclipseDuration = duration;
                umbraEntry = null;
            }

            // Track penumbra events
            if (inPenumbra && !wasInPenumbra)
            {
                penumbraEntry = currentEpoch;
            }
            else if (!inPenumbra && wasInPenumbra && penumbraEntry.HasValue)
            {
                var duration = currentEpoch - penumbraEntry.Value;
                events.Add(new EclipseEvent
                {
                    Type = EclipseType.Penumbra,
                    EntryTime = penumbraEntry.Value,
                    ExitTime = currentEpoch
                });
                totalPenumbra += duration;
                penumbraEntry = null;
            }

            wasInUmbra = inUmbra;
            wasInPenumbra = inPenumbra;
            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var totalTime = endEpoch - startEpoch;
        var sunlitTime = totalTime - totalUmbra - totalPenumbra;

        var result = new EclipseData
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Points = points,
            Events = events,
            Summary = new EclipseSummary
            {
                TotalEclipseEvents = events.Count,
                TotalUmbraDuration = totalUmbra,
                TotalPenumbraDuration = totalPenumbra,
                SunlitPercentage = sunlitTime.TotalSeconds / totalTime.TotalSeconds * 100,
                MaxEclipseDuration = maxEclipseDuration
            }
        };

        return Result<EclipseData>.Success(result);
    }

    /// <summary>
    /// Generate attitude visualization data
    /// </summary>
    public Result<AttitudeData> GenerateAttitudeData(
        Guid spacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? initialState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<AttitudeData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<AttitudePoint>();
        var state = initialState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagatedState = PropagateKepler(state, elapsedSeconds);

            // Calculate nadir-pointing attitude (simplified)
            var (qx, qy, qz, qw, roll, pitch, yaw) = CalculateNadirAttitude(propagatedState);
            var (sunX, sunY, sunZ) = GetSunVectorInBody(propagatedState, currentEpoch);
            var (earthX, earthY, earthZ) = GetEarthVectorInBody(propagatedState);

            points.Add(new AttitudePoint
            {
                Epoch = currentEpoch,
                Qx = qx,
                Qy = qy,
                Qz = qz,
                Qw = qw,
                RollDeg = roll,
                PitchDeg = pitch,
                YawDeg = yaw,
                SunVectorX = sunX,
                SunVectorY = sunY,
                SunVectorZ = sunZ,
                EarthVectorX = earthX,
                EarthVectorY = earthY,
                EarthVectorZ = earthZ
            });

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        var result = new AttitudeData
        {
            SpacecraftId = spacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            PointCount = points.Count,
            Points = points
        };

        return Result<AttitudeData>.Success(result);
    }

    /// <summary>
    /// Generate conjunction analysis data
    /// </summary>
    public Result<ConjunctionData> GenerateConjunctionData(
        Guid primarySpacecraftId,
        Guid secondarySpacecraftId,
        DateTime startEpoch,
        DateTime endEpoch,
        double sampleIntervalSeconds = 60,
        double[]? primaryState = null,
        double[]? secondaryState = null)
    {
        if (endEpoch <= startEpoch)
        {
            return Result<ConjunctionData>.Failure(Error.Validation("End epoch must be after start epoch"));
        }

        var points = new List<ConjunctionPoint>();
        var state1 = primaryState ?? new double[] { 7000, 0, 0, 0, 7.546, 0 };
        var state2 = secondaryState ?? new double[] { 7100, 100, 0, 0, 7.5, 0.1 };

        ConjunctionEvent? closestApproach = null;
        double minDistance = double.MaxValue;

        var currentEpoch = startEpoch;
        while (currentEpoch <= endEpoch)
        {
            var elapsedSeconds = (currentEpoch - startEpoch).TotalSeconds;
            var propagated1 = PropagateKepler(state1, elapsedSeconds);
            var propagated2 = PropagateKepler(state2, elapsedSeconds);

            var relX = propagated2[0] - propagated1[0];
            var relY = propagated2[1] - propagated1[1];
            var relZ = propagated2[2] - propagated1[2];
            var distance = Math.Sqrt(relX * relX + relY * relY + relZ * relZ);

            var relVx = propagated2[3] - propagated1[3];
            var relVy = propagated2[4] - propagated1[4];
            var relVz = propagated2[5] - propagated1[5];
            var relSpeed = Math.Sqrt(relVx * relVx + relVy * relVy + relVz * relVz);

            points.Add(new ConjunctionPoint
            {
                Epoch = currentEpoch,
                RelativeX = relX,
                RelativeY = relY,
                RelativeZ = relZ,
                DistanceKm = distance,
                RelativeSpeedKmps = relSpeed
            });

            if (distance < minDistance)
            {
                minDistance = distance;
                closestApproach = new ConjunctionEvent
                {
                    Epoch = currentEpoch,
                    MinDistanceKm = distance,
                    RelativeSpeedKmps = relSpeed,
                    TimeToClosestApproachSeconds = 0
                };
            }

            currentEpoch = currentEpoch.AddSeconds(sampleIntervalSeconds);
        }

        // Update time to closest approach for all points
        if (closestApproach != null)
        {
            var caEpoch = closestApproach.Epoch;
            foreach (var point in points)
            {
                point.TimeToClosestApproachSeconds = (caEpoch - point.Epoch).TotalSeconds;
            }
        }

        var result = new ConjunctionData
        {
            PrimarySpacecraftId = primarySpacecraftId,
            SecondarySpacecraftId = secondarySpacecraftId,
            StartEpoch = startEpoch,
            EndEpoch = endEpoch,
            Points = points,
            ClosestApproach = closestApproach
        };

        return Result<ConjunctionData>.Success(result);
    }

    // Helper methods

    private double[] PropagateKepler(double[] state, double dt)
    {
        var r = new double[] { state[0], state[1], state[2] };
        var v = new double[] { state[3], state[4], state[5] };

        var rMag = Math.Sqrt(r[0] * r[0] + r[1] * r[1] + r[2] * r[2]);
        var vMag = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

        // Specific energy
        var energy = vMag * vMag / 2 - EarthGM / rMag;

        // Semi-major axis
        var a = -EarthGM / (2 * energy);

        // Eccentricity vector
        var h = new double[]
        {
            r[1] * v[2] - r[2] * v[1],
            r[2] * v[0] - r[0] * v[2],
            r[0] * v[1] - r[1] * v[0]
        };
        var hMag = Math.Sqrt(h[0] * h[0] + h[1] * h[1] + h[2] * h[2]);

        var eVec = new double[]
        {
            (v[1] * h[2] - v[2] * h[1]) / EarthGM - r[0] / rMag,
            (v[2] * h[0] - v[0] * h[2]) / EarthGM - r[1] / rMag,
            (v[0] * h[1] - v[1] * h[0]) / EarthGM - r[2] / rMag
        };
        var e = Math.Sqrt(eVec[0] * eVec[0] + eVec[1] * eVec[1] + eVec[2] * eVec[2]);

        // Mean motion
        var n = Math.Sqrt(EarthGM / (a * a * a));

        // True anomaly at epoch
        var cosNu = (a * (1 - e * e) - rMag) / (e * rMag);
        cosNu = Math.Max(-1, Math.Min(1, cosNu));
        var nu0 = Math.Acos(cosNu);
        if (r[0] * v[0] + r[1] * v[1] + r[2] * v[2] < 0) nu0 = 2 * Math.PI - nu0;

        // Mean anomaly at epoch
        var E0 = 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(nu0 / 2));
        var M0 = E0 - e * Math.Sin(E0);

        // Mean anomaly at time t
        var M = M0 + n * dt;
        M = M % (2 * Math.PI);
        if (M < 0) M += 2 * Math.PI;

        // Solve Kepler's equation for E
        var E = SolveKepler(M, e);

        // True anomaly at time t
        var nu = 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(E / 2));

        // Radius at time t
        var rNew = a * (1 - e * Math.Cos(E));

        // Perifocal coordinates
        var pX = rNew * Math.Cos(nu);
        var pY = rNew * Math.Sin(nu);
        var pVx = -Math.Sqrt(EarthGM / (a * (1 - e * e))) * Math.Sin(nu);
        var pVy = Math.Sqrt(EarthGM / (a * (1 - e * e))) * (e + Math.Cos(nu));

        // Convert to ECI using rotation matrices
        // For simplicity, assuming orbit is in equatorial plane with RAAN = 0, omega = 0
        // This is a simplified propagation
        var angle = nu - nu0;
        var cosA = Math.Cos(angle);
        var sinA = Math.Sin(angle);

        return new double[]
        {
            r[0] * cosA - r[1] * sinA * Math.Sign(h[2]),
            r[0] * sinA * Math.Sign(h[2]) + r[1] * cosA,
            r[2],
            v[0] * cosA - v[1] * sinA * Math.Sign(h[2]),
            v[0] * sinA * Math.Sign(h[2]) + v[1] * cosA,
            v[2]
        };
    }

    private double SolveKepler(double M, double e)
    {
        var E = M;
        for (int i = 0; i < 50; i++)
        {
            var f = E - e * Math.Sin(E) - M;
            var df = 1 - e * Math.Cos(E);
            var dE = -f / df;
            E += dE;
            if (Math.Abs(dE) < 1e-12) break;
        }
        return E;
    }

    private (double a, double e, double i, double raan, double omega, double nu) StateToElements(double[] state)
    {
        var r = new double[] { state[0], state[1], state[2] };
        var v = new double[] { state[3], state[4], state[5] };

        var rMag = Math.Sqrt(r[0] * r[0] + r[1] * r[1] + r[2] * r[2]);
        var vMag = Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);

        var energy = vMag * vMag / 2 - EarthGM / rMag;
        var a = -EarthGM / (2 * energy);

        var h = new double[]
        {
            r[1] * v[2] - r[2] * v[1],
            r[2] * v[0] - r[0] * v[2],
            r[0] * v[1] - r[1] * v[0]
        };
        var hMag = Math.Sqrt(h[0] * h[0] + h[1] * h[1] + h[2] * h[2]);

        var i = Math.Acos(h[2] / hMag);

        var n = new double[] { -h[1], h[0], 0 };
        var nMag = Math.Sqrt(n[0] * n[0] + n[1] * n[1]);

        var raan = nMag > 1e-10 ? Math.Acos(n[0] / nMag) : 0;
        if (n[1] < 0) raan = 2 * Math.PI - raan;

        var eVec = new double[]
        {
            (v[1] * h[2] - v[2] * h[1]) / EarthGM - r[0] / rMag,
            (v[2] * h[0] - v[0] * h[2]) / EarthGM - r[1] / rMag,
            (v[0] * h[1] - v[1] * h[0]) / EarthGM - r[2] / rMag
        };
        var e = Math.Sqrt(eVec[0] * eVec[0] + eVec[1] * eVec[1] + eVec[2] * eVec[2]);

        var omega = 0.0;
        if (nMag > 1e-10 && e > 1e-10)
        {
            omega = Math.Acos((n[0] * eVec[0] + n[1] * eVec[1]) / (nMag * e));
            if (eVec[2] < 0) omega = 2 * Math.PI - omega;
        }

        var nu = 0.0;
        if (e > 1e-10)
        {
            nu = Math.Acos((eVec[0] * r[0] + eVec[1] * r[1] + eVec[2] * r[2]) / (e * rMag));
            if (r[0] * v[0] + r[1] * v[1] + r[2] * v[2] < 0) nu = 2 * Math.PI - nu;
        }

        return (a, e, i, raan, omega, nu);
    }

    private double TrueToMeanAnomaly(double nu, double e)
    {
        var E = 2 * Math.Atan(Math.Sqrt((1 - e) / (1 + e)) * Math.Tan(nu / 2));
        return E - e * Math.Sin(E);
    }

    private (double lat, double lon, double alt) EciToGeodetic(double[] state, DateTime epoch)
    {
        var x = state[0];
        var y = state[1];
        var z = state[2];

        // Earth rotation angle (simplified)
        var j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var daysSinceJ2000 = (epoch - j2000).TotalDays;
        var gmst = 280.46061837 + 360.98564736629 * daysSinceJ2000;
        gmst = gmst % 360;
        if (gmst < 0) gmst += 360;
        var gmstRad = gmst * Math.PI / 180;

        // Rotate to ECEF
        var xEcef = x * Math.Cos(gmstRad) + y * Math.Sin(gmstRad);
        var yEcef = -x * Math.Sin(gmstRad) + y * Math.Cos(gmstRad);
        var zEcef = z;

        // Geodetic conversion (simplified spherical)
        var r = Math.Sqrt(xEcef * xEcef + yEcef * yEcef + zEcef * zEcef);
        var lat = Math.Asin(zEcef / r) * 180 / Math.PI;
        var lon = Math.Atan2(yEcef, xEcef) * 180 / Math.PI;
        var alt = r - EarthRadiusKm;

        return (lat, lon, alt);
    }

    private bool IsDaylit(double[] state, DateTime epoch)
    {
        var sunDir = GetSunDirection(epoch);
        var r = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);

        // Dot product with sun direction
        var sunDot = (state[0] * sunDir.x + state[1] * sunDir.y + state[2] * sunDir.z) / r;
        return sunDot > 0;  // Simplified: daylit if sun is "above" the spacecraft
    }

    private (double x, double y, double z) GetSunDirection(DateTime epoch)
    {
        // Simplified sun position (circular orbit)
        var j2000 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var daysSinceJ2000 = (epoch - j2000).TotalDays;
        var meanLongitude = 280.46 + 0.9856474 * daysSinceJ2000;
        meanLongitude = meanLongitude % 360;
        var sunLon = meanLongitude * Math.PI / 180;
        var obliquity = 23.439 * Math.PI / 180;

        var x = Math.Cos(sunLon);
        var y = Math.Cos(obliquity) * Math.Sin(sunLon);
        var z = Math.Sin(obliquity) * Math.Sin(sunLon);

        return (x, y, z);
    }

    private (bool inUmbra, bool inPenumbra, double sunVisibility, double sunAngle) CalculateEclipse(double[] state, DateTime epoch)
    {
        var sunDir = GetSunDirection(epoch);
        var r = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);

        // Sun direction vector
        var sunDot = state[0] * sunDir.x + state[1] * sunDir.y + state[2] * sunDir.z;

        // Sun angle from spacecraft
        var sunAngle = Math.Acos(sunDot / r) * 180 / Math.PI;

        // Check if Earth blocks the sun
        var earthSunAngle = Math.Asin(EarthRadiusKm / r) * 180 / Math.PI;

        bool inUmbra = sunDot < 0 && sunAngle > (180 - earthSunAngle);
        bool inPenumbra = sunDot < 0 && sunAngle > (180 - earthSunAngle * 1.1) && !inUmbra;

        double sunVisibility = 100;
        if (inUmbra) sunVisibility = 0;
        else if (inPenumbra) sunVisibility = 50;

        return (inUmbra, inPenumbra, sunVisibility, sunAngle);
    }

    private (double qx, double qy, double qz, double qw, double roll, double pitch, double yaw) CalculateNadirAttitude(double[] state)
    {
        // Nadir-pointing: Z-axis towards Earth center
        var r = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);

        // Z-axis (nadir)
        var zx = -state[0] / r;
        var zy = -state[1] / r;
        var zz = -state[2] / r;

        // Y-axis (orbit normal, approximate)
        var hx = state[1] * state[5] - state[2] * state[4];
        var hy = state[2] * state[3] - state[0] * state[5];
        var hz = state[0] * state[4] - state[1] * state[3];
        var hMag = Math.Sqrt(hx * hx + hy * hy + hz * hz);
        var yx = hx / hMag;
        var yy = hy / hMag;
        var yz = hz / hMag;

        // X-axis (velocity direction)
        var xx = yy * zz - yz * zy;
        var xy = yz * zx - yx * zz;
        var xz = yx * zy - yy * zx;

        // Convert rotation matrix to quaternion (simplified)
        var trace = xx + yy + zz;
        double qw, qx, qy, qz;
        if (trace > 0)
        {
            var s = 0.5 / Math.Sqrt(trace + 1.0);
            qw = 0.25 / s;
            qx = (yz - zy) * s;
            qy = (zx - xz) * s;
            qz = (xy - yx) * s;
        }
        else
        {
            qw = 1; qx = 0; qy = 0; qz = 0;
        }

        // Convert to Euler angles (simplified)
        var roll = Math.Atan2(2 * (qw * qx + qy * qz), 1 - 2 * (qx * qx + qy * qy)) * 180 / Math.PI;
        var pitch = Math.Asin(2 * (qw * qy - qz * qx)) * 180 / Math.PI;
        var yaw = Math.Atan2(2 * (qw * qz + qx * qy), 1 - 2 * (qy * qy + qz * qz)) * 180 / Math.PI;

        return (qx, qy, qz, qw, roll, pitch, yaw);
    }

    private (double x, double y, double z) GetSunVectorInBody(double[] state, DateTime epoch)
    {
        var sunDir = GetSunDirection(epoch);
        // Simplified: return sun direction in inertial frame
        return (sunDir.x, sunDir.y, sunDir.z);
    }

    private (double x, double y, double z) GetEarthVectorInBody(double[] state)
    {
        var r = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);
        return (-state[0] / r, -state[1] / r, -state[2] / r);
    }

    private double CalculateStdDev(List<double> values)
    {
        if (values.Count < 2) return 0;
        var mean = values.Average();
        var sumSquares = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumSquares / (values.Count - 1));
    }
}
