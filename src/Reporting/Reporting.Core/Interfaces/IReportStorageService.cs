namespace Reporting.Core.Interfaces;

/// <summary>
/// Interface for report file storage
/// </summary>
public interface IReportStorageService
{
    /// <summary>
    /// Save report content to storage
    /// </summary>
    Task<string> SaveReportAsync(Guid reportId, byte[] content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve report content from storage
    /// </summary>
    Task<byte[]?> GetReportAsync(string storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete report from storage
    /// </summary>
    Task DeleteReportAsync(string storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if report exists in storage
    /// </summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get storage path for a report
    /// </summary>
    string GetStoragePath(Guid reportId, string fileName);
}
