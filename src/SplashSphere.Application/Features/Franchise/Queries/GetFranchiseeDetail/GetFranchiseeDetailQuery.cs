using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetFranchiseeDetail;

public sealed record GetFranchiseeDetailQuery(string FranchiseeTenantId) : IQuery<FranchiseeDetailDto>;
