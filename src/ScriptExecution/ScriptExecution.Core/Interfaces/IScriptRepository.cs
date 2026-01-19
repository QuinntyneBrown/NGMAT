using ScriptExecution.Core.Models;

namespace ScriptExecution.Core.Interfaces;

public interface IScriptRepository
{
    Task<Script?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Script>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Script>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Script script, CancellationToken cancellationToken = default);
    Task UpdateAsync(Script script, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
