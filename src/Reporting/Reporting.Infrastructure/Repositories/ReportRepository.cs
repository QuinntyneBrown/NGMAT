using Microsoft.EntityFrameworkCore;
using Reporting.Core.Entities;
using Reporting.Core.Interfaces;
using Reporting.Infrastructure.Persistence;

namespace Reporting.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly ReportDbContext _context;

    public ReportRepository(ReportDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.MissionId == missionId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.SpacecraftId == spacecraftId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
    }

    public Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        _context.Reports.Update(report);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var report = await _context.Reports.FindAsync(new object[] { id }, cancellationToken);
        if (report != null)
        {
            report.Delete();
        }
    }
}

public sealed class ReportTemplateRepository : IReportTemplateRepository
{
    private readonly ReportDbContext _context;

    public ReportTemplateRepository(ReportDbContext context)
    {
        _context = context;
    }

    public async Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<ReportTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates
            .FirstOrDefaultAsync(t => t.Name == name && !t.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<ReportTemplate>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates
            .Where(t => t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportTemplates
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ReportTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.ReportTemplates.AddAsync(template, cancellationToken);
    }

    public Task UpdateAsync(ReportTemplate template, CancellationToken cancellationToken = default)
    {
        _context.ReportTemplates.Update(template);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.ReportTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template != null)
        {
            template.Delete();
        }
    }
}

public sealed class ReportingUnitOfWork : IReportingUnitOfWork
{
    private readonly ReportDbContext _context;

    public IReportRepository Reports { get; }
    public IReportTemplateRepository Templates { get; }

    public ReportingUnitOfWork(ReportDbContext context)
    {
        _context = context;
        Reports = new ReportRepository(context);
        Templates = new ReportTemplateRepository(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
