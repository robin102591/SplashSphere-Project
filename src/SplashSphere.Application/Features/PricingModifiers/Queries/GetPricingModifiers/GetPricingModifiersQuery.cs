using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.PricingModifiers;

namespace SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifiers;

/// <summary>
/// Returns all pricing modifiers for the current tenant.
/// Optional filters: <paramref name="BranchId"/> (null = tenant-wide + all branch modifiers),
/// <paramref name="Type"/> (ModifierType int value), <paramref name="ActiveOnly"/>.
/// </summary>
public sealed record GetPricingModifiersQuery(
    string? BranchId = null,
    int? Type = null,
    bool? ActiveOnly = null) : IQuery<IReadOnlyList<PricingModifierDto>>;
