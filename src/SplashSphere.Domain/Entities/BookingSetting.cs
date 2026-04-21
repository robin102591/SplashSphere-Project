namespace SplashSphere.Domain.Entities;

/// <summary>
/// Tenant + branch-scoped configuration for online booking behaviour
/// (operating hours, slot capacity, lead time, etc.).
/// <para>
/// Unique on <c>(TenantId, BranchId)</c> — one settings row per branch.
/// Creation is lazy/upsert — defaults applied when the admin first saves.
/// </para>
/// </summary>
public sealed class BookingSetting : IAuditableEntity
{
    private BookingSetting() { } // EF Core

    public BookingSetting(string tenantId, string branchId)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;

    /// <summary>Local Manila time the branch opens for bookings (default 08:00).</summary>
    public TimeOnly OpenTime { get; set; } = new(8, 0);

    /// <summary>Local Manila time the branch stops accepting bookings (default 18:00).</summary>
    public TimeOnly CloseTime { get; set; } = new(18, 0);

    /// <summary>Slot granularity in minutes (default 30).</summary>
    public int SlotIntervalMinutes { get; set; } = 30;

    /// <summary>Max concurrent bookings per slot — driven by branch bay capacity (default 3).</summary>
    public int MaxBookingsPerSlot { get; set; } = 3;

    /// <summary>How many days in advance customers can book (default 7).</summary>
    public int AdvanceBookingDays { get; set; } = 7;

    /// <summary>Minimum minutes between now and the slot start (default 120 = 2 hours).</summary>
    public int MinLeadTimeMinutes { get; set; } = 120;

    /// <summary>Grace period past the slot before the booking is auto-marked NoShow (default 15).</summary>
    public int NoShowGraceMinutes { get; set; } = 15;

    /// <summary>Whether the branch accepts bookings at all.</summary>
    public bool IsBookingEnabled { get; set; } = true;

    /// <summary>
    /// Whether this branch appears in the Connect app's public car-wash directory.
    /// Used later for discovery filtering (see 22.2).
    /// </summary>
    public bool ShowInPublicDirectory { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
