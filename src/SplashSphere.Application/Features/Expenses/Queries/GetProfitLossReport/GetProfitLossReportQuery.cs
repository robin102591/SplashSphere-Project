using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Expenses.Queries.GetProfitLossReport;

public sealed record GetProfitLossReportQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<ProfitLossReportDto>;
