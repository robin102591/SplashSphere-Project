namespace SplashSphere.Application.Features.BookingAdmin;

/// <summary>A single service line on a booking, admin view.</summary>
public sealed record BookingAdminServiceDto(
    string ServiceId,
    string Name,
    decimal? Price,
    decimal? PriceMin,
    decimal? PriceMax);

/// <summary>Summary row for the admin bookings list.</summary>
public sealed record BookingListItemDto(
    string Id,
    string BranchId,
    string BranchName,
    string CustomerId,
    string CustomerName,
    string VehicleId,
    string PlateNumber,
    string ServiceSummary,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string Status,
    bool IsVehicleClassified,
    decimal EstimatedTotal,
    decimal? EstimatedTotalMin,
    decimal? EstimatedTotalMax,
    string? QueueEntryId,
    string? TransactionId);

/// <summary>Detail record returned by the admin booking-detail endpoint.</summary>
public sealed record BookingAdminDetailDto(
    string Id,
    string BranchId,
    string BranchName,
    string CustomerId,
    string CustomerName,
    string? CustomerPhone,
    string VehicleId,
    string PlateNumber,
    string? MakeName,
    string? ModelName,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    int EstimatedDurationMinutes,
    string Status,
    bool IsVehicleClassified,
    decimal EstimatedTotal,
    decimal? EstimatedTotalMin,
    decimal? EstimatedTotalMax,
    string? CancellationReason,
    string? QueueEntryId,
    string? TransactionId,
    DateTime CreatedAtUtc,
    IReadOnlyList<BookingAdminServiceDto> Services);
