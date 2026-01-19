using Shared.Domain.Results;

namespace CalculationEngine.Core.Services;

/// <summary>
/// Service for quaternion operations.
/// </summary>
public sealed class QuaternionService
{
    public Result<QuaternionResult> Multiply(QuaternionInput a, QuaternionInput b)
    {
        try
        {
            // Quaternion multiplication: q1 * q2
            // q = (w, x, y, z)
            var result = new QuaternionResult
            {
                W = a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
                X = a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                Y = a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                Z = a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W
            };
            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Quaternion multiplication failed: {ex.Message}");
        }
    }

    public Result<QuaternionResult> Conjugate(QuaternionInput q)
    {
        return new QuaternionResult
        {
            W = q.W,
            X = -q.X,
            Y = -q.Y,
            Z = -q.Z
        };
    }

    public Result<QuaternionResult> Inverse(QuaternionInput q)
    {
        try
        {
            var normSq = q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z;
            if (normSq < 1e-15)
            {
                return Error.Validation("Cannot invert zero quaternion");
            }

            return new QuaternionResult
            {
                W = q.W / normSq,
                X = -q.X / normSq,
                Y = -q.Y / normSq,
                Z = -q.Z / normSq
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Quaternion inverse failed: {ex.Message}");
        }
    }

    public Result<QuaternionResult> Normalize(QuaternionInput q)
    {
        try
        {
            var norm = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
            if (norm < 1e-15)
            {
                return Error.Validation("Cannot normalize zero quaternion");
            }

            return new QuaternionResult
            {
                W = q.W / norm,
                X = q.X / norm,
                Y = q.Y / norm,
                Z = q.Z / norm
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Quaternion normalization failed: {ex.Message}");
        }
    }

    public Result<double[][]> ToRotationMatrix(QuaternionInput q)
    {
        try
        {
            // Normalize first
            var norm = Math.Sqrt(q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z);
            var w = q.W / norm;
            var x = q.X / norm;
            var y = q.Y / norm;
            var z = q.Z / norm;

            // 3x3 rotation matrix
            var matrix = new double[3][];
            matrix[0] = new double[] { 1 - 2 * (y * y + z * z), 2 * (x * y - w * z), 2 * (x * z + w * y) };
            matrix[1] = new double[] { 2 * (x * y + w * z), 1 - 2 * (x * x + z * z), 2 * (y * z - w * x) };
            matrix[2] = new double[] { 2 * (x * z - w * y), 2 * (y * z + w * x), 1 - 2 * (x * x + y * y) };

            return matrix;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Quaternion to rotation matrix failed: {ex.Message}");
        }
    }

    public Result<QuaternionResult> FromRotationMatrix(double[][] matrix)
    {
        try
        {
            var trace = matrix[0][0] + matrix[1][1] + matrix[2][2];

            QuaternionResult result;

            if (trace > 0)
            {
                var s = 0.5 / Math.Sqrt(trace + 1.0);
                result = new QuaternionResult
                {
                    W = 0.25 / s,
                    X = (matrix[2][1] - matrix[1][2]) * s,
                    Y = (matrix[0][2] - matrix[2][0]) * s,
                    Z = (matrix[1][0] - matrix[0][1]) * s
                };
            }
            else if (matrix[0][0] > matrix[1][1] && matrix[0][0] > matrix[2][2])
            {
                var s = 2.0 * Math.Sqrt(1.0 + matrix[0][0] - matrix[1][1] - matrix[2][2]);
                result = new QuaternionResult
                {
                    W = (matrix[2][1] - matrix[1][2]) / s,
                    X = 0.25 * s,
                    Y = (matrix[0][1] + matrix[1][0]) / s,
                    Z = (matrix[0][2] + matrix[2][0]) / s
                };
            }
            else if (matrix[1][1] > matrix[2][2])
            {
                var s = 2.0 * Math.Sqrt(1.0 + matrix[1][1] - matrix[0][0] - matrix[2][2]);
                result = new QuaternionResult
                {
                    W = (matrix[0][2] - matrix[2][0]) / s,
                    X = (matrix[0][1] + matrix[1][0]) / s,
                    Y = 0.25 * s,
                    Z = (matrix[1][2] + matrix[2][1]) / s
                };
            }
            else
            {
                var s = 2.0 * Math.Sqrt(1.0 + matrix[2][2] - matrix[0][0] - matrix[1][1]);
                result = new QuaternionResult
                {
                    W = (matrix[1][0] - matrix[0][1]) / s,
                    X = (matrix[0][2] + matrix[2][0]) / s,
                    Y = (matrix[1][2] + matrix[2][1]) / s,
                    Z = 0.25 * s
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            return Error.Validation($"Rotation matrix to quaternion failed: {ex.Message}");
        }
    }

    public Result<QuaternionResult> Slerp(QuaternionInput a, QuaternionInput b, double t)
    {
        try
        {
            // Compute dot product
            var dot = a.W * b.W + a.X * b.X + a.Y * b.Y + a.Z * b.Z;

            // If dot is negative, negate one quaternion to take shorter path
            var bw = b.W;
            var bx = b.X;
            var by = b.Y;
            var bz = b.Z;

            if (dot < 0)
            {
                dot = -dot;
                bw = -bw;
                bx = -bx;
                by = -by;
                bz = -bz;
            }

            double s0, s1;

            if (dot > 0.9995)
            {
                // Linear interpolation for nearly parallel quaternions
                s0 = 1 - t;
                s1 = t;
            }
            else
            {
                var theta = Math.Acos(dot);
                var sinTheta = Math.Sin(theta);
                s0 = Math.Sin((1 - t) * theta) / sinTheta;
                s1 = Math.Sin(t * theta) / sinTheta;
            }

            return new QuaternionResult
            {
                W = s0 * a.W + s1 * bw,
                X = s0 * a.X + s1 * bx,
                Y = s0 * a.Y + s1 * by,
                Z = s0 * a.Z + s1 * bz
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"SLERP failed: {ex.Message}");
        }
    }

    public Result<QuaternionResult> FromEulerAngles(double roll, double pitch, double yaw)
    {
        try
        {
            // Convert Euler angles (ZYX convention) to quaternion
            var cy = Math.Cos(yaw * 0.5);
            var sy = Math.Sin(yaw * 0.5);
            var cp = Math.Cos(pitch * 0.5);
            var sp = Math.Sin(pitch * 0.5);
            var cr = Math.Cos(roll * 0.5);
            var sr = Math.Sin(roll * 0.5);

            return new QuaternionResult
            {
                W = cr * cp * cy + sr * sp * sy,
                X = sr * cp * cy - cr * sp * sy,
                Y = cr * sp * cy + sr * cp * sy,
                Z = cr * cp * sy - sr * sp * cy
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Euler to quaternion conversion failed: {ex.Message}");
        }
    }

    public Result<EulerAnglesResult> ToEulerAngles(QuaternionInput q)
    {
        try
        {
            // Quaternion to Euler angles (ZYX convention)
            var sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            var cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            var roll = Math.Atan2(sinr_cosp, cosr_cosp);

            var sinp = Math.Sqrt(1 + 2 * (q.W * q.Y - q.X * q.Z));
            var cosp = Math.Sqrt(1 - 2 * (q.W * q.Y - q.X * q.Z));
            var pitch = 2 * Math.Atan2(sinp, cosp) - Math.PI / 2;

            var siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            var cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            var yaw = Math.Atan2(siny_cosp, cosy_cosp);

            return new EulerAnglesResult
            {
                Roll = roll,
                Pitch = pitch,
                Yaw = yaw
            };
        }
        catch (Exception ex)
        {
            return Error.Validation($"Quaternion to Euler conversion failed: {ex.Message}");
        }
    }
}

public sealed class QuaternionInput
{
    public double W { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public sealed class QuaternionResult
{
    public double W { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public sealed class EulerAnglesResult
{
    public double Roll { get; init; }
    public double Pitch { get; init; }
    public double Yaw { get; init; }
}
