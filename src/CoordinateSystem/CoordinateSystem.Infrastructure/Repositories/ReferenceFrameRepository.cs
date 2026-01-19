using CoordinateSystem.Core.Entities;
using CoordinateSystem.Core.Interfaces;
using CoordinateSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoordinateSystem.Infrastructure.Repositories;

public sealed class ReferenceFrameRepository : IReferenceFrameRepository
{
    private readonly CoordinateSystemDbContext _context;

    public ReferenceFrameRepository(CoordinateSystemDbContext context)
    {
        _context = context;
    }

    public async Task<ReferenceFrame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Check built-in frames first
        var builtIn = BuiltInFrames.GetAll().FirstOrDefault(f => f.Id == id);
        if (builtIn != null)
            return builtIn;

        return await _context.ReferenceFrames
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<ReferenceFrame?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Check built-in frames first
        var builtIn = BuiltInFrames.GetAll().FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (builtIn != null)
            return builtIn;

        return await _context.ReferenceFrames
            .FirstOrDefaultAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceFrame>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customFrames = await _context.ReferenceFrames.ToListAsync(cancellationToken);
        var builtInFrames = BuiltInFrames.GetAll();
        return builtInFrames.Concat(customFrames).ToList();
    }

    public async Task<IReadOnlyList<ReferenceFrame>> GetByCentralBodyAsync(CentralBody centralBody, CancellationToken cancellationToken = default)
    {
        var customFrames = await _context.ReferenceFrames
            .Where(f => f.CentralBody == centralBody)
            .ToListAsync(cancellationToken);

        var builtInFrames = BuiltInFrames.GetAll().Where(f => f.CentralBody == centralBody);

        return builtInFrames.Concat(customFrames).ToList();
    }

    public Task<IReadOnlyList<ReferenceFrame>> GetBuiltInFramesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ReferenceFrame>>(BuiltInFrames.GetAll().ToList());
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // Check built-in frames first
        if (BuiltInFrames.GetAll().Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return true;

        return await _context.ReferenceFrames
            .AnyAsync(f => f.Name == name, cancellationToken);
    }

    public async Task AddAsync(ReferenceFrame frame, CancellationToken cancellationToken = default)
    {
        await _context.ReferenceFrames.AddAsync(frame, cancellationToken);
    }

    public Task UpdateAsync(ReferenceFrame frame, CancellationToken cancellationToken = default)
    {
        _context.ReferenceFrames.Update(frame);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var frame = await _context.ReferenceFrames.FindAsync(new object[] { id }, cancellationToken);
        if (frame != null && !frame.IsBuiltIn)
        {
            _context.ReferenceFrames.Remove(frame);
        }
    }
}

public sealed class CoordinateSystemUnitOfWork : ICoordinateSystemUnitOfWork
{
    private readonly CoordinateSystemDbContext _context;

    public IReferenceFrameRepository ReferenceFrames { get; }

    public CoordinateSystemUnitOfWork(CoordinateSystemDbContext context)
    {
        _context = context;
        ReferenceFrames = new ReferenceFrameRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
