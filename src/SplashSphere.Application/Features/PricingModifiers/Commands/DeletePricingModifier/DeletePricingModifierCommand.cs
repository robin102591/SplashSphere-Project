using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.DeletePricingModifier;

public sealed record DeletePricingModifierCommand(string Id) : ICommand;
