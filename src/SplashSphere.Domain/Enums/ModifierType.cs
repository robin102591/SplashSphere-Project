namespace SplashSphere.Domain.Enums;

/// <summary>
/// Determines what condition activates a pricing modifier and how its
/// <c>value</c> field is interpreted.
/// </summary>
public enum ModifierType
{
    /// <summary>
    /// Active during configured hours of the day (e.g. 06:00–09:00 morning rush).
    /// <c>value</c> is a percentage multiplier applied to the base price (e.g. <c>1.20</c> = +20%).
    /// </summary>
    PeakHour = 1,

    /// <summary>
    /// Active on a specific day of the week or a recurring weekly schedule.
    /// <c>value</c> is a percentage multiplier (e.g. <c>0.90</c> = -10% weekend discount).
    /// </summary>
    DayOfWeek = 2,

    /// <summary>
    /// Active on a named public holiday in the Philippine holiday calendar.
    /// <c>value</c> is a percentage multiplier.
    /// </summary>
    Holiday = 3,

    /// <summary>
    /// A manually triggered promotional discount with explicit start/end dates.
    /// <c>value</c> is an absolute peso discount subtracted from the base price.
    /// </summary>
    Promotion = 4,

    /// <summary>
    /// Adjustment based on weather conditions (e.g. rainy season surcharge for
    /// deep-cleaning services). Activated manually by branch staff.
    /// <c>value</c> is a percentage multiplier.
    /// </summary>
    Weather = 5,
}
