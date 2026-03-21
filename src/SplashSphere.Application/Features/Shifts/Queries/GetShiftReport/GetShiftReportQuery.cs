using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftReport;

/// <summary>Returns the full end-of-day report for a shift including top services and employees.</summary>
public sealed record GetShiftReportQuery(string ShiftId) : IQuery<ShiftReportDto?>;
