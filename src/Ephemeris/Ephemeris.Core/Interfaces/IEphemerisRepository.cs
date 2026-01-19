using Ephemeris.Core.Entities;

namespace Ephemeris.Core.Interfaces;

public interface ICelestialBodyRepository
{
    Task<CelestialBody?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CelestialBody?> GetByNaifIdAsync(int naifId, CancellationToken cancellationToken = default);
    Task<CelestialBody?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CelestialBody>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CelestialBody>> GetByTypeAsync(CelestialBodyType type, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNaifIdAsync(int naifId, CancellationToken cancellationToken = default);
    Task AddAsync(CelestialBody body, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CelestialBody> bodies, CancellationToken cancellationToken = default);
    Task UpdateAsync(CelestialBody body, CancellationToken cancellationToken = default);
}

public interface ICelestialBodyPositionRepository
{
    Task<CelestialBodyPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CelestialBodyPosition?> GetAtEpochAsync(Guid celestialBodyId, DateTime epoch, int centerNaifId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CelestialBodyPosition>> GetInRangeAsync(Guid celestialBodyId, DateTime startEpoch, DateTime endEpoch, int centerNaifId, CancellationToken cancellationToken = default);
    Task<(CelestialBodyPosition? before, CelestialBodyPosition? after)> GetBoundingPositionsAsync(Guid celestialBodyId, DateTime epoch, int centerNaifId, CancellationToken cancellationToken = default);
    Task AddAsync(CelestialBodyPosition position, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CelestialBodyPosition> positions, CancellationToken cancellationToken = default);
}

public interface IEarthOrientationParametersRepository
{
    Task<EarthOrientationParameters?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EarthOrientationParameters?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<EarthOrientationParameters?> GetAtMjdAsync(double mjd, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EarthOrientationParameters>> GetInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<(EarthOrientationParameters? before, EarthOrientationParameters? after)> GetBoundingParametersAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<EarthOrientationParameters?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task AddAsync(EarthOrientationParameters eop, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<EarthOrientationParameters> eops, CancellationToken cancellationToken = default);
    Task UpdateAsync(EarthOrientationParameters eop, CancellationToken cancellationToken = default);
}

public interface ISpaceWeatherDataRepository
{
    Task<SpaceWeatherData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SpaceWeatherData?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpaceWeatherData>> GetInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<SpaceWeatherData?> GetLatestAsync(CancellationToken cancellationToken = default);
    Task<double> GetF107Average81DayAsync(DateTime centerDate, CancellationToken cancellationToken = default);
    Task AddAsync(SpaceWeatherData data, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<SpaceWeatherData> data, CancellationToken cancellationToken = default);
    Task UpdateAsync(SpaceWeatherData data, CancellationToken cancellationToken = default);
}

public interface ILeapSecondRepository
{
    Task<IReadOnlyList<LeapSecond>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LeapSecond?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<double> GetTaiMinusUtcAsync(DateTime utc, CancellationToken cancellationToken = default);
    Task AddAsync(LeapSecond leapSecond, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<LeapSecond> leapSeconds, CancellationToken cancellationToken = default);
}

public interface IEphemerisUnitOfWork
{
    ICelestialBodyRepository CelestialBodies { get; }
    ICelestialBodyPositionRepository CelestialBodyPositions { get; }
    IEarthOrientationParametersRepository EarthOrientationParameters { get; }
    ISpaceWeatherDataRepository SpaceWeatherData { get; }
    ILeapSecondRepository LeapSeconds { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
