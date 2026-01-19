using Ephemeris.Core.Entities;
using Ephemeris.Core.Events;
using Ephemeris.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Ephemeris.Core.Services;

public sealed class EphemerisService
{
    private readonly IEphemerisUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public EphemerisService(IEphemerisUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<CelestialBody>> CreateCelestialBodyAsync(
        string name,
        int naifId,
        CelestialBodyType type,
        double gravitationalParameterM3S2,
        double meanRadiusKm,
        double? equatorialRadiusKm = null,
        double? polarRadiusKm = null,
        double? flatteningCoefficient = null,
        double? j2Coefficient = null,
        double? rotationPeriodSeconds = null,
        Guid? parentBodyId = null,
        CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.CelestialBodies.ExistsByNaifIdAsync(naifId, cancellationToken))
        {
            return Result<CelestialBody>.Failure(Error.Conflict($"A celestial body with NAIF ID {naifId} already exists"));
        }

        var body = CelestialBody.Create(
            name, naifId, type, gravitationalParameterM3S2, meanRadiusKm,
            equatorialRadiusKm, polarRadiusKm, flatteningCoefficient,
            j2Coefficient, rotationPeriodSeconds, parentBodyId);

        await _unitOfWork.CelestialBodies.AddAsync(body, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new CelestialBodyCreatedEvent
        {
            CelestialBodyId = body.Id,
            Name = body.Name,
            NaifId = body.NaifId,
            Type = body.Type.ToString()
        }, cancellationToken);

        return Result<CelestialBody>.Success(body);
    }

    public async Task<Result<CelestialBody>> GetCelestialBodyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var body = await _unitOfWork.CelestialBodies.GetByIdAsync(id, cancellationToken);
        if (body == null)
        {
            return Result<CelestialBody>.Failure(Error.NotFound("CelestialBody", id.ToString()));
        }
        return Result<CelestialBody>.Success(body);
    }

    public async Task<Result<CelestialBody>> GetCelestialBodyByNaifIdAsync(int naifId, CancellationToken cancellationToken = default)
    {
        var body = await _unitOfWork.CelestialBodies.GetByNaifIdAsync(naifId, cancellationToken);
        if (body == null)
        {
            return Result<CelestialBody>.Failure(Error.NotFound("CelestialBody.NaifId", naifId.ToString()));
        }
        return Result<CelestialBody>.Success(body);
    }

    public async Task<Result<IReadOnlyList<CelestialBody>>> GetAllCelestialBodiesAsync(CancellationToken cancellationToken = default)
    {
        var bodies = await _unitOfWork.CelestialBodies.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CelestialBody>>.Success(bodies);
    }

    public async Task<Result<CelestialBodyPosition>> RecordPositionAsync(
        Guid celestialBodyId,
        DateTime epoch,
        double x, double y, double z,
        double vx, double vy, double vz,
        int centerNaifId,
        string source,
        string referenceFrame = "ICRF",
        double? ax = null, double? ay = null, double? az = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _unitOfWork.CelestialBodies.GetByIdAsync(celestialBodyId, cancellationToken);
        if (body == null)
        {
            return Result<CelestialBodyPosition>.Failure(Error.NotFound("CelestialBody", celestialBodyId.ToString()));
        }

        var position = CelestialBodyPosition.Create(
            celestialBodyId, epoch, x, y, z, vx, vy, vz,
            centerNaifId, source, referenceFrame, ax, ay, az);

        await _unitOfWork.CelestialBodyPositions.AddAsync(position, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new CelestialBodyPositionRecordedEvent
        {
            PositionId = position.Id,
            CelestialBodyId = celestialBodyId,
            Epoch = epoch,
            Source = source
        }, cancellationToken);

        return Result<CelestialBodyPosition>.Success(position);
    }

    public async Task<Result<CelestialBodyPosition>> GetPositionAtEpochAsync(
        Guid celestialBodyId,
        DateTime epoch,
        int centerNaifId,
        bool interpolate = true,
        CancellationToken cancellationToken = default)
    {
        // First try exact match
        var exactMatch = await _unitOfWork.CelestialBodyPositions.GetAtEpochAsync(
            celestialBodyId, epoch, centerNaifId, cancellationToken);

        if (exactMatch != null)
        {
            return Result<CelestialBodyPosition>.Success(exactMatch);
        }

        if (!interpolate)
        {
            return Result<CelestialBodyPosition>.Failure(Error.NotFound("CelestialBodyPosition", $"{celestialBodyId} at {epoch:O}"));
        }

        // Try to interpolate
        var (before, after) = await _unitOfWork.CelestialBodyPositions.GetBoundingPositionsAsync(
            celestialBodyId, epoch, centerNaifId, cancellationToken);

        if (before == null || after == null)
        {
            return Result<CelestialBodyPosition>.Failure(Error.NotFound("CelestialBodyPosition", $"No data for interpolation at {epoch:O}"));
        }

        var interpolated = InterpolatePosition(before, after, epoch);
        return Result<CelestialBodyPosition>.Success(interpolated);
    }

    public async Task<Result<CelestialBodyPosition>> GetPositionByNaifIdAtEpochAsync(
        int naifId,
        DateTime epoch,
        int centerNaifId,
        bool interpolate = true,
        CancellationToken cancellationToken = default)
    {
        var body = await _unitOfWork.CelestialBodies.GetByNaifIdAsync(naifId, cancellationToken);
        if (body == null)
        {
            return Result<CelestialBodyPosition>.Failure(Error.NotFound("CelestialBody.NaifId", naifId.ToString()));
        }

        return await GetPositionAtEpochAsync(body.Id, epoch, centerNaifId, interpolate, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<CelestialBodyPosition>>> GetPositionsInRangeAsync(
        Guid celestialBodyId,
        DateTime startEpoch,
        DateTime endEpoch,
        int centerNaifId,
        CancellationToken cancellationToken = default)
    {
        var positions = await _unitOfWork.CelestialBodyPositions.GetInRangeAsync(
            celestialBodyId, startEpoch, endEpoch, centerNaifId, cancellationToken);

        return Result<IReadOnlyList<CelestialBodyPosition>>.Success(positions);
    }

    public async Task<Result<int>> ImportStandardBodiesAsync(CancellationToken cancellationToken = default)
    {
        var standardBodies = StandardCelestialBodies.GetStandardBodies().ToList();
        var importedCount = 0;

        foreach (var body in standardBodies)
        {
            if (!await _unitOfWork.CelestialBodies.ExistsByNaifIdAsync(body.NaifId, cancellationToken))
            {
                await _unitOfWork.CelestialBodies.AddAsync(body, cancellationToken);
                importedCount++;
            }
        }

        if (importedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(importedCount);
    }

    private static CelestialBodyPosition InterpolatePosition(
        CelestialBodyPosition before,
        CelestialBodyPosition after,
        DateTime epoch)
    {
        var totalSeconds = (after.Epoch - before.Epoch).TotalSeconds;
        var elapsedSeconds = (epoch - before.Epoch).TotalSeconds;
        var fraction = elapsedSeconds / totalSeconds;

        // Hermite interpolation for position using position and velocity at both endpoints
        var h00 = 2 * fraction * fraction * fraction - 3 * fraction * fraction + 1;
        var h10 = fraction * fraction * fraction - 2 * fraction * fraction + fraction;
        var h01 = -2 * fraction * fraction * fraction + 3 * fraction * fraction;
        var h11 = fraction * fraction * fraction - fraction * fraction;

        var dt = totalSeconds;

        var x = h00 * before.X + h10 * dt * before.Vx + h01 * after.X + h11 * dt * after.Vx;
        var y = h00 * before.Y + h10 * dt * before.Vy + h01 * after.Y + h11 * dt * after.Vy;
        var z = h00 * before.Z + h10 * dt * before.Vz + h01 * after.Z + h11 * dt * after.Vz;

        // Derivative of Hermite polynomial for velocity
        var dh00 = (6 * fraction * fraction - 6 * fraction) / dt;
        var dh10 = 3 * fraction * fraction - 4 * fraction + 1;
        var dh01 = (-6 * fraction * fraction + 6 * fraction) / dt;
        var dh11 = 3 * fraction * fraction - 2 * fraction;

        var vx = dh00 * before.X + dh10 * before.Vx + dh01 * after.X + dh11 * after.Vx;
        var vy = dh00 * before.Y + dh10 * before.Vy + dh01 * after.Y + dh11 * after.Vy;
        var vz = dh00 * before.Z + dh10 * before.Vz + dh01 * after.Z + dh11 * after.Vz;

        return CelestialBodyPosition.Create(
            before.CelestialBodyId,
            epoch,
            x, y, z,
            vx, vy, vz,
            before.CenterNaifId,
            $"Interpolated({before.Source})",
            before.ReferenceFrame);
    }
}
