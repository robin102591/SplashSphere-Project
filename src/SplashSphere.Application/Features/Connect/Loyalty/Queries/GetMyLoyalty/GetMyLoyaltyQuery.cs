using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetMyLoyalty;

/// <summary>
/// Read the authenticated Connect customer's membership at a single tenant.
/// Returns <see cref="ConnectMembershipDto"/> with <c>IsEnrolled = false</c>
/// (and zeroes) when the tenant doesn't offer loyalty or the customer hasn't
/// earned a card yet.
/// </summary>
public sealed record GetMyLoyaltyQuery(string TenantId) : IQuery<ConnectMembershipDto>;
