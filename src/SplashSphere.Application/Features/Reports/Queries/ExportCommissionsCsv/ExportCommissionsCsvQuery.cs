using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.ExportCommissionsCsv;

public sealed record ExportCommissionsCsvQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null,
    string? EmployeeId = null) : IQuery<ReportCsvResult>;
