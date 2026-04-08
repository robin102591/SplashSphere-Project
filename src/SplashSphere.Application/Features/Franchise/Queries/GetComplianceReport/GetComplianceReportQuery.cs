using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetComplianceReport;

public sealed record GetComplianceReportQuery : IQuery<IReadOnlyList<FranchiseComplianceItemDto>>;
