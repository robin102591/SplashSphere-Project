using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.ExportServicePopularityCsv;

public sealed record ExportServicePopularityCsvQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null,
    int Top = 20) : IQuery<ReportCsvResult>;
