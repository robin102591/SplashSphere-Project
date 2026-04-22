namespace SplashSphere.Application.Features.Connect.Booking;

/// <summary>A single available slot on a given branch+date.</summary>
public sealed record BookingSlotDto(
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string LocalTime,
    int RemainingCapacity);

/// <summary>
/// A service line on a booking as returned to the Connect app. When
/// <c>Price</c> is set the vehicle was classified at booking time; otherwise
/// the <c>PriceMin</c>/<c>PriceMax</c> range is shown.
/// </summary>
public sealed record BookingServiceDto(
    string ServiceId,
    string Name,
    decimal? Price,
    decimal? PriceMin,
    decimal? PriceMax);

/// <summary>A booking summary for list views.</summary>
public sealed record BookingListItemDto(
    string Id,
    string TenantId,
    string TenantName,
    string BranchId,
    string BranchName,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string Status,
    bool IsVehicleClassified,
    decimal EstimatedTotal,
    decimal? EstimatedTotalMin,
    decimal? EstimatedTotalMax,
    string VehicleId,
    string PlateNumber);

/// <summary>Detail view for a booking — summary fields plus services + queue status.</summary>
public sealed record BookingDetailDto(
    string Id,
    string TenantId,
    string TenantName,
    string BranchId,
    string BranchName,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string Status,
    bool IsVehicleClassified,
    decimal EstimatedTotal,
    decimal? EstimatedTotalMin,
    decimal? EstimatedTotalMax,
    int EstimatedDurationMinutes,
    string VehicleId,
    string PlateNumber,
    string? QueueEntryId,
    string? QueueNumber,
    string? QueueStatus,
    string? TransactionId,
    IReadOnlyList<BookingServiceDto> Services);
