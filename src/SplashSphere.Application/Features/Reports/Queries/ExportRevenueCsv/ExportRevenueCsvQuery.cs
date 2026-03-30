using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.ExportRevenueCsv;

public sealed record ExportRevenueCsvQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<ReportCsvResult>;
