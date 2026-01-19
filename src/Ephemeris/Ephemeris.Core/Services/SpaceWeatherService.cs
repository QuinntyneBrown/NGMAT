using Ephemeris.Core.Entities;
using Ephemeris.Core.Events;
using Ephemeris.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;

namespace Ephemeris.Core.Services;

public sealed class SpaceWeatherService
{
    private readonly IEphemerisUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public SpaceWeatherService(IEphemerisUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<SpaceWeatherData>> RecordSpaceWeatherAsync(
        DateTime date,
        double f107Observed,
        double f107Adjusted,
        double f107Average81Day,
        double apDaily,
        double kpSum,
        string source,
        bool isPrediction,
        double[]? ap3Hour = null,
        double[]? kp3Hour = null,
        double? sunspotNumber = null,
        double? mgIiIndex = null,
        double? s107 = null,
        double? m107 = null,
        double? y107 = null,
        double? dstIndex = null,
        CancellationToken cancellationToken = default)
    {
        var data = SpaceWeatherData.Create(
            date, f107Observed, f107Adjusted, f107Average81Day, apDaily, kpSum, source, isPrediction,
            ap3Hour, kp3Hour, sunspotNumber, mgIiIndex, s107, m107, y107, dstIndex);

        await _unitOfWork.SpaceWeatherData.AddAsync(data, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SpaceWeatherDataUpdatedEvent
        {
            Date = date,
            Source = source,
            F107 = f107Observed,
            Ap = apDaily
        }, cancellationToken);

        return Result<SpaceWeatherData>.Success(data);
    }

    public async Task<Result<SpaceWeatherData>> GetSpaceWeatherAtDateAsync(
        DateTime date,
        bool interpolate = true,
        CancellationToken cancellationToken = default)
    {
        var exactMatch = await _unitOfWork.SpaceWeatherData.GetAtDateAsync(date.Date, cancellationToken);
        if (exactMatch != null)
        {
            return Result<SpaceWeatherData>.Success(exactMatch);
        }

        if (!interpolate)
        {
            return Result<SpaceWeatherData>.Failure(Error.NotFound("SpaceWeatherData", date.ToString("O")));
        }

        // For space weather, we typically use nearest available data
        var latest = await _unitOfWork.SpaceWeatherData.GetLatestAsync(cancellationToken);
        if (latest != null)
        {
            return Result<SpaceWeatherData>.Success(latest);
        }

        // Return average values if no data available
        var defaultData = SpaceWeatherData.Create(
            date,
            SpaceWeatherConstants.AverageF107,
            SpaceWeatherConstants.AverageF107,
            SpaceWeatherConstants.AverageF107,
            SpaceWeatherConstants.AverageAp,
            SpaceWeatherConstants.AverageAp * 8.0 / 3.0, // Approximate Kp sum
            "Default",
            true);

        return Result<SpaceWeatherData>.Success(defaultData);
    }

    public async Task<Result<IReadOnlyList<SpaceWeatherData>>> GetSpaceWeatherInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var data = await _unitOfWork.SpaceWeatherData.GetInRangeAsync(startDate, endDate, cancellationToken);
        return Result<IReadOnlyList<SpaceWeatherData>>.Success(data);
    }

    public async Task<Result<AtmosphericIndices>> GetAtmosphericIndicesAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var result = await GetSpaceWeatherAtDateAsync(date, true, cancellationToken);
        if (result.IsFailure)
        {
            // Return default values
            return Result<AtmosphericIndices>.Success(new AtmosphericIndices
            {
                Date = date,
                F107 = SpaceWeatherConstants.AverageF107,
                F107Average = SpaceWeatherConstants.AverageF107,
                Ap = SpaceWeatherConstants.AverageAp,
                Kp = SpaceWeatherConstants.AverageAp / 8.0 // Approximate conversion
            });
        }

        var data = result.Value!;
        return Result<AtmosphericIndices>.Success(new AtmosphericIndices
        {
            Date = date,
            F107 = data.F107Observed,
            F107Average = data.F107Average81Day,
            Ap = data.ApDaily,
            Kp = data.KpSum / 8.0 // Average Kp for the day
        });
    }

    public async Task<Result<int>> ImportSpaceWeatherDataAsync(
        IEnumerable<SpaceWeatherData> data,
        CancellationToken cancellationToken = default)
    {
        var dataList = data.ToList();
        if (dataList.Count == 0)
        {
            return Result<int>.Success(0);
        }

        await _unitOfWork.SpaceWeatherData.AddRangeAsync(dataList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var startDate = dataList.Min(d => d.Date);
        var endDate = dataList.Max(d => d.Date);

        await _eventPublisher.PublishAsync(new EphemerisDataImportedEvent
        {
            DataType = "SpaceWeatherData",
            Source = dataList.First().Source,
            RecordCount = dataList.Count,
            StartDate = startDate,
            EndDate = endDate
        }, cancellationToken);

        return Result<int>.Success(dataList.Count);
    }
}

public sealed class AtmosphericIndices
{
    public DateTime Date { get; init; }
    public double F107 { get; init; }
    public double F107Average { get; init; }
    public double Ap { get; init; }
    public double Kp { get; init; }
}
