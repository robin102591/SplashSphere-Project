using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftVarianceReport;

public sealed record GetShiftVarianceReportQuery(
    string? BranchId,
    string? CashierId,
    DateOnly? DateFrom,
    DateOnly? DateTo) : IQuery<ShiftVarianceReportDto>;

public sealed record ShiftVarianceReportDto(
    IReadOnlyList<ShiftVarianceCashierDto> CashierSummaries,
    IReadOnlyList<VarianceTrendPointDto>? TrendPoints);
