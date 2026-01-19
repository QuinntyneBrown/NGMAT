using Microsoft.EntityFrameworkCore;
using Ephemeris.Core.Entities;
using Ephemeris.Core.Interfaces;
using Ephemeris.Infrastructure.Persistence;

namespace Ephemeris.Infrastructure.Repositories;

public sealed class CelestialBodyRepository : ICelestialBodyRepository
{
    private readonly EphemerisDbContext _context;

    public CelestialBodyRepository(EphemerisDbContext context)
    {
        _context = context;
    }

    public async Task<CelestialBody?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies
            .Include(c => c.ParentBody)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CelestialBody?> GetByNaifIdAsync(int naifId, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies
            .Include(c => c.ParentBody)
            .FirstOrDefaultAsync(c => c.NaifId == naifId, cancellationToken);
    }

    public async Task<CelestialBody?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies
            .Include(c => c.ParentBody)
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<CelestialBody>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies
            .Include(c => c.ParentBody)
            .OrderBy(c => c.NaifId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CelestialBody>> GetByTypeAsync(CelestialBodyType type, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies
            .Include(c => c.ParentBody)
            .Where(c => c.Type == type)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNaifIdAsync(int naifId, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodies.AnyAsync(c => c.NaifId == naifId, cancellationToken);
    }

    public async Task AddAsync(CelestialBody body, CancellationToken cancellationToken = default)
    {
        await _context.CelestialBodies.AddAsync(body, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CelestialBody> bodies, CancellationToken cancellationToken = default)
    {
        await _context.CelestialBodies.AddRangeAsync(bodies, cancellationToken);
    }

    public Task UpdateAsync(CelestialBody body, CancellationToken cancellationToken = default)
    {
        _context.CelestialBodies.Update(body);
        return Task.CompletedTask;
    }
}

public sealed class CelestialBodyPositionRepository : ICelestialBodyPositionRepository
{
    private readonly EphemerisDbContext _context;

    public CelestialBodyPositionRepository(EphemerisDbContext context)
    {
        _context = context;
    }

    public async Task<CelestialBodyPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodyPositions
            .Include(p => p.CelestialBody)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<CelestialBodyPosition?> GetAtEpochAsync(Guid celestialBodyId, DateTime epoch, int centerNaifId, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodyPositions
            .Include(p => p.CelestialBody)
            .FirstOrDefaultAsync(p => p.CelestialBodyId == celestialBodyId && p.Epoch == epoch && p.CenterNaifId == centerNaifId, cancellationToken);
    }

    public async Task<IReadOnlyList<CelestialBodyPosition>> GetInRangeAsync(Guid celestialBodyId, DateTime startEpoch, DateTime endEpoch, int centerNaifId, CancellationToken cancellationToken = default)
    {
        return await _context.CelestialBodyPositions
            .Include(p => p.CelestialBody)
            .Where(p => p.CelestialBodyId == celestialBodyId && p.Epoch >= startEpoch && p.Epoch <= endEpoch && p.CenterNaifId == centerNaifId)
            .OrderBy(p => p.Epoch)
            .ToListAsync(cancellationToken);
    }

    public async Task<(CelestialBodyPosition? before, CelestialBodyPosition? after)> GetBoundingPositionsAsync(Guid celestialBodyId, DateTime epoch, int centerNaifId, CancellationToken cancellationToken = default)
    {
        var before = await _context.CelestialBodyPositions
            .Where(p => p.CelestialBodyId == celestialBodyId && p.Epoch <= epoch && p.CenterNaifId == centerNaifId)
            .OrderByDescending(p => p.Epoch)
            .FirstOrDefaultAsync(cancellationToken);

        var after = await _context.CelestialBodyPositions
            .Where(p => p.CelestialBodyId == celestialBodyId && p.Epoch >= epoch && p.CenterNaifId == centerNaifId)
            .OrderBy(p => p.Epoch)
            .FirstOrDefaultAsync(cancellationToken);

        return (before, after);
    }

    public async Task AddAsync(CelestialBodyPosition position, CancellationToken cancellationToken = default)
    {
        await _context.CelestialBodyPositions.AddAsync(position, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CelestialBodyPosition> positions, CancellationToken cancellationToken = default)
    {
        await _context.CelestialBodyPositions.AddRangeAsync(positions, cancellationToken);
    }
}

public sealed class EarthOrientationParametersRepository : IEarthOrientationParametersRepository
{
    private readonly EphemerisDbContext _context;

    public EarthOrientationParametersRepository(EphemerisDbContext context)
    {
        _context = context;
    }

    public async Task<EarthOrientationParameters?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EarthOrientationParameters.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<EarthOrientationParameters?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.EarthOrientationParameters
            .FirstOrDefaultAsync(e => e.Date.Date == date.Date, cancellationToken);
    }

    public async Task<EarthOrientationParameters?> GetAtMjdAsync(double mjd, CancellationToken cancellationToken = default)
    {
        return await _context.EarthOrientationParameters
            .FirstOrDefaultAsync(e => Math.Abs(e.Mjd - mjd) < 0.5, cancellationToken);
    }

    public async Task<IReadOnlyList<EarthOrientationParameters>> GetInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.EarthOrientationParameters
            .Where(e => e.Date >= startDate && e.Date <= endDate)
            .OrderBy(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<(EarthOrientationParameters? before, EarthOrientationParameters? after)> GetBoundingParametersAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var before = await _context.EarthOrientationParameters
            .Where(e => e.Date <= date)
            .OrderByDescending(e => e.Date)
            .FirstOrDefaultAsync(cancellationToken);

        var after = await _context.EarthOrientationParameters
            .Where(e => e.Date >= date)
            .OrderBy(e => e.Date)
            .FirstOrDefaultAsync(cancellationToken);

        return (before, after);
    }

    public async Task<EarthOrientationParameters?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EarthOrientationParameters
            .OrderByDescending(e => e.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(EarthOrientationParameters eop, CancellationToken cancellationToken = default)
    {
        await _context.EarthOrientationParameters.AddAsync(eop, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<EarthOrientationParameters> eops, CancellationToken cancellationToken = default)
    {
        await _context.EarthOrientationParameters.AddRangeAsync(eops, cancellationToken);
    }

    public Task UpdateAsync(EarthOrientationParameters eop, CancellationToken cancellationToken = default)
    {
        _context.EarthOrientationParameters.Update(eop);
        return Task.CompletedTask;
    }
}

public sealed class SpaceWeatherDataRepository : ISpaceWeatherDataRepository
{
    private readonly EphemerisDbContext _context;

    public SpaceWeatherDataRepository(EphemerisDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceWeatherData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SpaceWeatherData.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<SpaceWeatherData?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.SpaceWeatherData
            .FirstOrDefaultAsync(s => s.Date.Date == date.Date, cancellationToken);
    }

    public async Task<IReadOnlyList<SpaceWeatherData>> GetInRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.SpaceWeatherData
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<SpaceWeatherData?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SpaceWeatherData
            .OrderByDescending(s => s.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<double> GetF107Average81DayAsync(DateTime centerDate, CancellationToken cancellationToken = default)
    {
        var startDate = centerDate.AddDays(-40);
        var endDate = centerDate.AddDays(40);

        var data = await _context.SpaceWeatherData
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .ToListAsync(cancellationToken);

        if (data.Count == 0)
        {
            return SpaceWeatherConstants.AverageF107;
        }

        return data.Average(s => s.F107Observed);
    }

    public async Task AddAsync(SpaceWeatherData data, CancellationToken cancellationToken = default)
    {
        await _context.SpaceWeatherData.AddAsync(data, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<SpaceWeatherData> data, CancellationToken cancellationToken = default)
    {
        await _context.SpaceWeatherData.AddRangeAsync(data, cancellationToken);
    }

    public Task UpdateAsync(SpaceWeatherData data, CancellationToken cancellationToken = default)
    {
        _context.SpaceWeatherData.Update(data);
        return Task.CompletedTask;
    }
}

public sealed class LeapSecondRepository : ILeapSecondRepository
{
    private readonly EphemerisDbContext _context;

    public LeapSecondRepository(EphemerisDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LeapSecond>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LeapSeconds
            .OrderBy(l => l.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeapSecond?> GetAtDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.LeapSeconds
            .Where(l => l.EffectiveDate <= date)
            .OrderByDescending(l => l.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<double> GetTaiMinusUtcAsync(DateTime utc, CancellationToken cancellationToken = default)
    {
        var leapSecond = await GetAtDateAsync(utc, cancellationToken);
        return leapSecond?.TaiMinusUtcSeconds ?? 10.0; // Default before 1972
    }

    public async Task AddAsync(LeapSecond leapSecond, CancellationToken cancellationToken = default)
    {
        await _context.LeapSeconds.AddAsync(leapSecond, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<LeapSecond> leapSeconds, CancellationToken cancellationToken = default)
    {
        await _context.LeapSeconds.AddRangeAsync(leapSeconds, cancellationToken);
    }
}

public sealed class EphemerisUnitOfWork : IEphemerisUnitOfWork
{
    private readonly EphemerisDbContext _context;

    public ICelestialBodyRepository CelestialBodies { get; }
    public ICelestialBodyPositionRepository CelestialBodyPositions { get; }
    public IEarthOrientationParametersRepository EarthOrientationParameters { get; }
    public ISpaceWeatherDataRepository SpaceWeatherData { get; }
    public ILeapSecondRepository LeapSeconds { get; }

    public EphemerisUnitOfWork(EphemerisDbContext context)
    {
        _context = context;
        CelestialBodies = new CelestialBodyRepository(context);
        CelestialBodyPositions = new CelestialBodyPositionRepository(context);
        EarthOrientationParameters = new EarthOrientationParametersRepository(context);
        SpaceWeatherData = new SpaceWeatherDataRepository(context);
        LeapSeconds = new LeapSecondRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
