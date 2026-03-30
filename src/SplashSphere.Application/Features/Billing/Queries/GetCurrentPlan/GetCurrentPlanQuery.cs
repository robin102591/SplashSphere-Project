using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Billing.Queries.GetCurrentPlan;

public sealed record GetCurrentPlanQuery : IQuery<TenantPlanDto>;
