using Microsoft.Extensions.Configuration;
using Reporting.Core.Interfaces;

namespace Reporting.Infrastructure.Storage;

/// <summary>
/// File system based report storage implementation
/// </summary>
public sealed class FileSystemReportStorage : IReportStorageService
{
    private readonly string _baseDirectory;

    public FileSystemReportStorage(IConfiguration configuration)
    {
        _baseDirectory = configuration["ReportStorage:BaseDirectory"] ?? Path.Combine(Path.GetTempPath(), "reports");
        
        // Ensure directory exists
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }
    }

    public async Task<string> SaveReportAsync(Guid reportId, byte[] content, string fileName, CancellationToken cancellationToken = default)
    {
        var storagePath = GetStoragePath(reportId, fileName);
        var fullPath = Path.Combine(_baseDirectory, storagePath);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        
        return storagePath;
    }

    public async Task<byte[]?> GetReportAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, storagePath);
        
        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteReportAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, storagePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseDirectory, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetStoragePath(Guid reportId, string fileName)
    {
        // Organize by year/month/day for easier management
        var now = DateTime.UtcNow;
        return Path.Combine(
            now.Year.ToString(),
            now.Month.ToString("D2"),
            now.Day.ToString("D2"),
            reportId.ToString("N"),
            fileName);
    }
}
