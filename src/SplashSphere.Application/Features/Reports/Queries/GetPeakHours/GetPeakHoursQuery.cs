using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetPeakHours;

public sealed record GetPeakHoursQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<PeakHoursDto>;
