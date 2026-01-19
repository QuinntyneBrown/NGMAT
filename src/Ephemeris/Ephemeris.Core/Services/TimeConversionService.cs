using Ephemeris.Core.Entities;
using Ephemeris.Core.Events;
using Ephemeris.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Ephemeris.Core.Services;

public sealed class TimeConversionService
{
    private readonly IEphemerisUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public TimeConversionService(IEphemerisUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<double>> GetTaiMinusUtcAsync(DateTime utc, CancellationToken cancellationToken = default)
    {
        var leapSeconds = await _unitOfWork.LeapSeconds.GetTaiMinusUtcAsync(utc, cancellationToken);
        return Result<double>.Success(leapSeconds);
    }

    public async Task<Result<DateTime>> UtcToTaiAsync(DateTime utc, CancellationToken cancellationToken = default)
    {
        var leapSeconds = await _unitOfWork.LeapSeconds.GetTaiMinusUtcAsync(utc, cancellationToken);
        return Result<DateTime>.Success(utc.AddSeconds(leapSeconds));
    }

    public async Task<Result<DateTime>> UtcToTtAsync(DateTime utc, CancellationToken cancellationToken = default)
    {
        var tai = await UtcToTaiAsync(utc, cancellationToken);
        if (tai.IsFailure)
        {
            return tai;
        }
        return Result<DateTime>.Success(tai.Value!.AddSeconds(LeapSecondData.TtMinusTaiSeconds));
    }

    public async Task<Result<DateTime>> UtcToUt1Async(DateTime utc, CancellationToken cancellationToken = default)
    {
        var eop = await _unitOfWork.EarthOrientationParameters.GetAtDateAsync(utc.Date, cancellationToken);
        var ut1MinusUtc = eop?.Ut1MinusUtcSeconds ?? 0.0;
        return Result<DateTime>.Success(utc.AddSeconds(ut1MinusUtc));
    }

    public Result<double> DateTimeToJulianDate(DateTime dt)
    {
        return Result<double>.Success(TimeConversions.DateTimeToJd(dt));
    }

    public Result<double> DateTimeToModifiedJulianDate(DateTime dt)
    {
        return Result<double>.Success(TimeConversions.DateTimeToMjd(dt));
    }

    public Result<DateTime> JulianDateToDateTime(double jd)
    {
        return Result<DateTime>.Success(TimeConversions.JdToDateTime(jd));
    }

    public Result<DateTime> ModifiedJulianDateToDateTime(double mjd)
    {
        return Result<DateTime>.Success(TimeConversions.MjdToDateTime(mjd));
    }

    public Result<double> GetJulianCenturiesFromJ2000(DateTime dt)
    {
        return Result<double>.Success(TimeConversions.JulianCenturiesFromJ2000(dt));
    }

    public async Task<Result<int>> ImportHistoricalLeapSecondsAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.LeapSeconds.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return Result<int>.Success(0);
        }

        var historicalLeapSeconds = LeapSecondData.GetHistoricalLeapSeconds()
            .Select(ls => LeapSecond.Create(ls.date, ls.taiMinusUtc, "IERS"))
            .ToList();

        await _unitOfWork.LeapSeconds.AddRangeAsync(historicalLeapSeconds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new EphemerisDataImportedEvent
        {
            DataType = "LeapSeconds",
            Source = "IERS",
            RecordCount = historicalLeapSeconds.Count,
            StartDate = historicalLeapSeconds.Min(ls => ls.EffectiveDate),
            EndDate = historicalLeapSeconds.Max(ls => ls.EffectiveDate)
        }, cancellationToken);

        return Result<int>.Success(historicalLeapSeconds.Count);
    }

    public async Task<Result<LeapSecond>> AddLeapSecondAsync(
        DateTime effectiveDate,
        double taiMinusUtcSeconds,
        string source,
        CancellationToken cancellationToken = default)
    {
        var leapSecond = LeapSecond.Create(effectiveDate, taiMinusUtcSeconds, source);

        await _unitOfWork.LeapSeconds.AddAsync(leapSecond, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new LeapSecondAddedEvent
        {
            EffectiveDate = effectiveDate,
            TaiMinusUtcSeconds = taiMinusUtcSeconds
        }, cancellationToken);

        return Result<LeapSecond>.Success(leapSecond);
    }

    public async Task<Result<IReadOnlyList<LeapSecond>>> GetAllLeapSecondsAsync(CancellationToken cancellationToken = default)
    {
        var leapSeconds = await _unitOfWork.LeapSeconds.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<LeapSecond>>.Success(leapSeconds);
    }
}
