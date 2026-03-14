using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.CreatePricingModifier;

public sealed record CreatePricingModifierCommand(
    string Name,
    ModifierType Type,
    decimal Value,
    string? BranchId,
    // PeakHour
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    // DayOfWeek
    DayOfWeek? ActiveDayOfWeek,
    // Holiday
    DateOnly? HolidayDate,
    string? HolidayName,
    // Promotion
    DateOnly? StartDate,
    DateOnly? EndDate) : ICommand<string>;
