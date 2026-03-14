using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.PricingModifiers;

public sealed record PricingModifierDto(
    string Id,
    string Name,
    ModifierType Type,
    string TypeLabel,
    decimal Value,
    string? BranchId,
    string? BranchName,
    // PeakHour fields
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    // DayOfWeek fields
    DayOfWeek? ActiveDayOfWeek,
    // Holiday fields
    DateOnly? HolidayDate,
    string? HolidayName,
    // Promotion fields
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
