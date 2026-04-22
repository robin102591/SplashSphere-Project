using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingAdmin.Queries.GetBookingDetailAdmin;

/// <summary>Admin-side booking detail fetch. Tenant-scoped via query filters.</summary>
public sealed record GetBookingDetailAdminQuery(string BookingId)
    : IQuery<BookingAdminDetailDto?>;
