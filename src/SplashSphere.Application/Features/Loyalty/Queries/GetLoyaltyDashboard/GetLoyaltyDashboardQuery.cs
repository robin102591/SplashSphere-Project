using MediatR;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;

public sealed record GetLoyaltyDashboardQuery(
    DateTime From,
    DateTime To) : IRequest<LoyaltyDashboardDto>;
