using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpsertServicePricing;

/// <summary>
/// Replaces the entire pricing matrix for a service in one atomic operation.
/// ServiceId comes from the route; Rows come from the request body.
/// An empty Rows list clears the matrix (service falls back to BasePrice for all lookups).
/// </summary>
public sealed record UpsertServicePricingCommand(
    string ServiceId,
    IReadOnlyList<ServicePricingRowRequest> Rows) : ICommand;

/// <summary>One cell in the pricing matrix PUT body.</summary>
public sealed record ServicePricingRowRequest(
    string VehicleTypeId,
    string SizeId,
    decimal Price);
