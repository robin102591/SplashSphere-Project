namespace SplashSphere.Domain.Entities;

/// <summary>
/// A single service line on a <see cref="Booking"/>.
/// <para>
/// Either <see cref="Price"/> is set (vehicle classified at booking time → exact price)
/// OR both <see cref="PriceMin"/> and <see cref="PriceMax"/> are set (not classified →
/// price range shown to the customer, final price confirmed by the cashier on arrival).
/// </para>
/// Tenant-scoped for isolation + query-filter alignment, even though the row is
/// owned by the parent Booking.
/// </summary>
public sealed class BookingService : IAuditableEntity
{
    private BookingService() { } // EF Core

    public BookingService(string tenantId, string bookingId, string serviceId)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BookingId = bookingId;
        ServiceId = serviceId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>Exact price — set when vehicle is classified.</summary>
    public decimal? Price { get; set; }

    /// <summary>Price range low — set when vehicle is not classified.</summary>
    public decimal? PriceMin { get; set; }

    /// <summary>Price range high — set when vehicle is not classified.</summary>
    public decimal? PriceMax { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Booking Booking { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
