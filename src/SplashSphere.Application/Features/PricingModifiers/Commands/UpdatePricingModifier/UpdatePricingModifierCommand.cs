using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.UpdatePricingModifier;

public sealed record UpdatePricingModifierCommand(
    string Id,
    string Name,
    ModifierType Type,
    decimal Value,
    string? BranchId,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    DayOfWeek? ActiveDayOfWeek,
    DateOnly? HolidayDate,
    string? HolidayName,
    DateOnly? StartDate,
    DateOnly? EndDate) : ICommand;
