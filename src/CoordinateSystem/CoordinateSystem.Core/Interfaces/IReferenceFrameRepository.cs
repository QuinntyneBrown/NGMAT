using CoordinateSystem.Core.Entities;

namespace CoordinateSystem.Core.Interfaces;

public interface IReferenceFrameRepository
{
    Task<ReferenceFrame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReferenceFrame?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferenceFrame>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferenceFrame>> GetByCentralBodyAsync(CentralBody centralBody, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferenceFrame>> GetBuiltInFramesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(ReferenceFrame frame, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReferenceFrame frame, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICoordinateSystemUnitOfWork
{
    IReferenceFrameRepository ReferenceFrames { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
