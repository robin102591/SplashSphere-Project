using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.TogglePricingModifierStatus;

public sealed record TogglePricingModifierStatusCommand(string Id) : ICommand;
