using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltySettings;

public sealed record UpsertLoyaltySettingsCommand(
    decimal PointsPerCurrencyUnit,
    decimal CurrencyUnitAmount,
    bool IsActive,
    int? PointsExpirationMonths,
    bool AutoEnroll) : ICommand;
