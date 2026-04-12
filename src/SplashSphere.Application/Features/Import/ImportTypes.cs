namespace SplashSphere.Application.Features.Import;

public enum ImportType
{
    Customers = 0,
    Vehicles = 1,
    Employees = 2,
    Services = 3,
}

public sealed record ColumnMapping(Dictionary<string, string> Mappings);

public sealed record ImportValidationResult
{
    public int TotalRows { get; init; }
    public int ValidRows { get; init; }
    public int WarningRows { get; init; }
    public int ErrorRows { get; init; }
    public List<ImportRowError> Errors { get; init; } = [];
    public List<ImportRowWarning> Warnings { get; init; } = [];
    public List<string> DetectedColumns { get; init; } = [];
    public List<Dictionary<string, string>> PreviewRows { get; init; } = [];
}

public sealed record ImportRowError(int Row, string Column, string Message);
public sealed record ImportRowWarning(int Row, string Column, string Message, string? CorrectedValue);

public sealed record ImportResult
{
    public int Imported { get; init; }
    public int Corrected { get; init; }
    public int Skipped { get; init; }
    public List<ImportRowError> SkippedErrors { get; init; } = [];
}

public sealed record ImportFileRequest(ImportType Type, Stream FileStream, string FileName, ColumnMapping? Mapping);
