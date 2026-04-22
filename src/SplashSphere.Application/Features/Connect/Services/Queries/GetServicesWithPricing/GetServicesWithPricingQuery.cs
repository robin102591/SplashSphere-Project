using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Services.Queries.GetServicesWithPricing;

/// <summary>
/// List the tenant's active services with pricing for the caller's selected
/// vehicle. If the plate matches an existing tenant <c>Car</c> (i.e. the
/// cashier classified it on a prior visit), returns exact prices for the
/// classified (type, size); otherwise returns a min/max range across all
/// pricing rows (with <c>basePrice</c> fallback when the matrix is empty).
/// </summary>
public sealed record GetServicesWithPricingQuery(
    string TenantId,
    string VehicleId) : IQuery<ConnectServicesWithPricingDto?>;
