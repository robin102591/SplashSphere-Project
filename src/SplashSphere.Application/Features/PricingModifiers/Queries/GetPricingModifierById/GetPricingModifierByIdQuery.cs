using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifierById;

public sealed record GetPricingModifierByIdQuery(string Id) : IQuery<PricingModifierDto?>;
