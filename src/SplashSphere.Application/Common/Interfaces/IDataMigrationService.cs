using SplashSphere.Application.Features.Import;

namespace SplashSphere.Application.Common.Interfaces;

public interface IDataMigrationService
{
    /// <summary>Parse the file and return detected columns + preview rows (no mapping needed yet).</summary>
    Task<ImportValidationResult> DetectColumnsAsync(ImportType type, Stream fileStream, string fileName, CancellationToken ct);

    /// <summary>Validate the file with column mappings applied. Returns row-level errors/warnings.</summary>
    Task<ImportValidationResult> ValidateAsync(ImportType type, Stream fileStream, string fileName, ColumnMapping mapping, CancellationToken ct);

    /// <summary>Execute the import. Transactional — rolls back on critical errors.</summary>
    Task<ImportResult> ExecuteAsync(ImportType type, Stream fileStream, string fileName, ColumnMapping mapping, CancellationToken ct);

    /// <summary>Generate a CSV template with headers and example rows for the given import type.</summary>
    byte[] GenerateTemplate(ImportType type);
}
