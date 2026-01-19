using Ephemeris.Core.Entities;
using Ephemeris.Core.Events;
using Ephemeris.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Ephemeris.Core.Services;

public sealed class EarthOrientationService
{
    private readonly IEphemerisUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public EarthOrientationService(IEphemerisUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<EarthOrientationParameters>> RecordEopAsync(
        double mjd,
        DateTime date,
        double xPoleArcsec,
        double yPoleArcsec,
        double ut1MinusUtcSeconds,
        double lodMilliseconds,
        double dPsiArcsec,
        double dEpsilonArcsec,
        string source,
        bool isPrediction,
        double? xPoleUncertainty = null,
        double? yPoleUncertainty = null,
        double? ut1MinusUtcUncertainty = null,
        CancellationToken cancellationToken = default)
    {
        var eop = EarthOrientationParameters.Create(
            mjd, date, xPoleArcsec, yPoleArcsec, ut1MinusUtcSeconds, lodMilliseconds,
            dPsiArcsec, dEpsilonArcsec, source, isPrediction,
            xPoleUncertainty, yPoleUncertainty, ut1MinusUtcUncertainty);

        await _unitOfWork.EarthOrientationParameters.AddAsync(eop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new EarthOrientationParametersUpdatedEvent
        {
            Date = date,
            Source = source,
            RecordCount = 1
        }, cancellationToken);

        return Result<EarthOrientationParameters>.Success(eop);
    }

    public async Task<Result<EarthOrientationParameters>> GetEopAtDateAsync(
        DateTime date,
        bool interpolate = true,
        CancellationToken cancellationToken = default)
    {
        var exactMatch = await _unitOfWork.EarthOrientationParameters.GetAtDateAsync(date.Date, cancellationToken);
        if (exactMatch != null)
        {
            return Result<EarthOrientationParameters>.Success(exactMatch);
        }

        if (!interpolate)
        {
            return Result<EarthOrientationParameters>.Failure(Error.NotFound("EarthOrientationParameters", date.ToString("O")));
        }

        var (before, after) = await _unitOfWork.EarthOrientationParameters.GetBoundingParametersAsync(date, cancellationToken);
        if (before == null || after == null)
        {
            return Result<EarthOrientationParameters>.Failure(Error.NotFound("EarthOrientationParameters", $"No data for interpolation at {date:O}"));
        }

        var interpolated = InterpolateEop(before, after, date);
        return Result<EarthOrientationParameters>.Success(interpolated);
    }

    public async Task<Result<IReadOnlyList<EarthOrientationParameters>>> GetEopInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var eops = await _unitOfWork.EarthOrientationParameters.GetInRangeAsync(startDate, endDate, cancellationToken);
        return Result<IReadOnlyList<EarthOrientationParameters>>.Success(eops);
    }

    public async Task<Result<double>> GetUt1MinusUtcAsync(
        DateTime utc,
        CancellationToken cancellationToken = default)
    {
        var result = await GetEopAtDateAsync(utc, true, cancellationToken);
        if (result.IsFailure)
        {
            // Return 0 as fallback if no EOP data available
            return Result<double>.Success(0.0);
        }

        return Result<double>.Success(result.Value!.Ut1MinusUtcSeconds);
    }

    public async Task<Result<(double xPole, double yPole)>> GetPolarMotionAsync(
        DateTime utc,
        CancellationToken cancellationToken = default)
    {
        var result = await GetEopAtDateAsync(utc, true, cancellationToken);
        if (result.IsFailure)
        {
            // Return 0 as fallback if no EOP data available
            return Result<(double, double)>.Success((0.0, 0.0));
        }

        return Result<(double, double)>.Success((result.Value!.XPoleArcsec, result.Value.YPoleArcsec));
    }

    public async Task<Result<int>> ImportEopDataAsync(
        IEnumerable<EarthOrientationParameters> eops,
        CancellationToken cancellationToken = default)
    {
        var eopList = eops.ToList();
        if (eopList.Count == 0)
        {
            return Result<int>.Success(0);
        }

        await _unitOfWork.EarthOrientationParameters.AddRangeAsync(eopList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var startDate = eopList.Min(e => e.Date);
        var endDate = eopList.Max(e => e.Date);

        await _eventPublisher.PublishAsync(new EphemerisDataImportedEvent
        {
            DataType = "EarthOrientationParameters",
            Source = eopList.First().Source,
            RecordCount = eopList.Count,
            StartDate = startDate,
            EndDate = endDate
        }, cancellationToken);

        return Result<int>.Success(eopList.Count);
    }

    private static EarthOrientationParameters InterpolateEop(
        EarthOrientationParameters before,
        EarthOrientationParameters after,
        DateTime date)
    {
        var totalDays = (after.Date - before.Date).TotalDays;
        var elapsedDays = (date - before.Date).TotalDays;
        var fraction = elapsedDays / totalDays;

        var mjd = before.Mjd + fraction * (after.Mjd - before.Mjd);
        var xPole = before.XPoleArcsec + fraction * (after.XPoleArcsec - before.XPoleArcsec);
        var yPole = before.YPoleArcsec + fraction * (after.YPoleArcsec - before.YPoleArcsec);
        var ut1MinusUtc = before.Ut1MinusUtcSeconds + fraction * (after.Ut1MinusUtcSeconds - before.Ut1MinusUtcSeconds);
        var lod = before.LodMilliseconds + fraction * (after.LodMilliseconds - before.LodMilliseconds);
        var dPsi = before.DPsiArcsec + fraction * (after.DPsiArcsec - before.DPsiArcsec);
        var dEpsilon = before.DEpsilonArcsec + fraction * (after.DEpsilonArcsec - before.DEpsilonArcsec);

        return EarthOrientationParameters.Create(
            mjd, date, xPole, yPole, ut1MinusUtc, lod, dPsi, dEpsilon,
            $"Interpolated({before.Source})", before.IsPrediction || after.IsPrediction);
    }
}
