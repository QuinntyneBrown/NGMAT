using CalculationEngine.Core.Services;

namespace CalculationEngine.Api.Endpoints;

public static class QuaternionEndpoints
{
    public static void MapQuaternionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quaternion")
            .WithTags("Quaternion Operations");

        group.MapPost("/multiply", async (QuaternionPairRequest request, QuaternionService service) =>
        {
            var a = new QuaternionInput { W = request.A.W, X = request.A.X, Y = request.A.Y, Z = request.A.Z };
            var b = new QuaternionInput { W = request.B.W, X = request.B.X, Y = request.B.Y, Z = request.B.Z };
            var result = service.Multiply(a, b);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("MultiplyQuaternions");

        group.MapPost("/conjugate", async (QuaternionRequest request, QuaternionService service) =>
        {
            var q = new QuaternionInput { W = request.W, X = request.X, Y = request.Y, Z = request.Z };
            var result = service.Conjugate(q);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("ConjugateQuaternion");

        group.MapPost("/inverse", async (QuaternionRequest request, QuaternionService service) =>
        {
            var q = new QuaternionInput { W = request.W, X = request.X, Y = request.Y, Z = request.Z };
            var result = service.Inverse(q);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("InverseQuaternion");

        group.MapPost("/normalize", async (QuaternionRequest request, QuaternionService service) =>
        {
            var q = new QuaternionInput { W = request.W, X = request.X, Y = request.Y, Z = request.Z };
            var result = service.Normalize(q);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("NormalizeQuaternion");

        group.MapPost("/to-rotation-matrix", async (QuaternionRequest request, QuaternionService service) =>
        {
            var q = new QuaternionInput { W = request.W, X = request.X, Y = request.Y, Z = request.Z };
            var result = service.ToRotationMatrix(q);
            return result.IsSuccess
                ? Results.Ok(new { RotationMatrix = result.Value })
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("QuaternionToRotationMatrix");

        group.MapPost("/from-rotation-matrix", async (RotationMatrixRequest request, QuaternionService service) =>
        {
            var result = service.FromRotationMatrix(request.Matrix);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("RotationMatrixToQuaternion");

        group.MapPost("/slerp", async (SlerpRequest request, QuaternionService service) =>
        {
            var a = new QuaternionInput { W = request.A.W, X = request.A.X, Y = request.A.Y, Z = request.A.Z };
            var b = new QuaternionInput { W = request.B.W, X = request.B.X, Y = request.B.Y, Z = request.B.Z };
            var result = service.Slerp(a, b, request.T);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("SlerpQuaternions");

        group.MapPost("/from-euler", async (EulerAnglesRequest request, QuaternionService service) =>
        {
            var result = service.FromEulerAngles(request.Roll, request.Pitch, request.Yaw);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("EulerToQuaternion");

        group.MapPost("/to-euler", async (QuaternionRequest request, QuaternionService service) =>
        {
            var q = new QuaternionInput { W = request.W, X = request.X, Y = request.Y, Z = request.Z };
            var result = service.ToEulerAngles(q);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { Error = result.Error.Message });
        }).WithName("QuaternionToEuler");
    }
}

public sealed class QuaternionRequest
{
    public double W { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
}

public sealed class QuaternionPairRequest
{
    public QuaternionRequest A { get; init; } = new();
    public QuaternionRequest B { get; init; } = new();
}

public sealed class RotationMatrixRequest
{
    public double[][] Matrix { get; init; } = Array.Empty<double[]>();
}

public sealed class SlerpRequest
{
    public QuaternionRequest A { get; init; } = new();
    public QuaternionRequest B { get; init; } = new();
    public double T { get; init; }
}

public sealed class EulerAnglesRequest
{
    public double Roll { get; init; }
    public double Pitch { get; init; }
    public double Yaw { get; init; }
}
