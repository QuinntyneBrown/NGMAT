using Reporting.Core.Entities;

namespace Reporting.Core.Interfaces;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetByMissionIdAsync(Guid missionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetBySpacecraftIdAsync(Guid spacecraftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Report>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IReportTemplateRepository
{
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ReportTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ReportTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReportTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IReportingUnitOfWork
{
    IReportRepository Reports { get; }
    IReportTemplateRepository Templates { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
