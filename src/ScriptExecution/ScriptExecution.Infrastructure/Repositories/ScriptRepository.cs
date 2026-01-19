using System.Collections.Concurrent;
using ScriptExecution.Core.Interfaces;
using ScriptExecution.Core.Models;

namespace ScriptExecution.Infrastructure.Repositories;

/// <summary>
/// In-memory script repository implementation
/// </summary>
public sealed class ScriptRepository : IScriptRepository
{
    private readonly ConcurrentDictionary<Guid, Script> _scripts = new();

    public Task<Script?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _scripts.TryGetValue(id, out var script);
        return Task.FromResult(script);
    }

    public Task<IReadOnlyList<Script>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var scripts = _scripts.Values
            .Where(s => s.CreatedByUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Script>>(scripts);
    }

    public Task<IReadOnlyList<Script>> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var scripts = _scripts.Values
            .Where(s => s.IsPublic)
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Script>>(scripts);
    }

    public Task AddAsync(Script script, CancellationToken cancellationToken = default)
    {
        _scripts.TryAdd(script.Id, script);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Script script, CancellationToken cancellationToken = default)
    {
        _scripts[script.Id] = script;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _scripts.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
