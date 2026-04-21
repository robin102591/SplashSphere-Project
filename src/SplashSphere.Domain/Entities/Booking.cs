namespace SplashSphere.Domain.Entities;

/// <summary>
/// An online booking created by a customer via the Connect app. Tenant-scoped.
/// <para>
/// <b>Pricing semantics:</b>
/// If the customer's vehicle is already classified at this tenant (i.e. a
/// <see cref="Car"/> record exists with VehicleType + Size), <see cref="IsVehicleClassified"/>
/// is <c>true</c> and <see cref="EstimatedTotal"/> is an exact figure.
/// Otherwise the booking stores a price <b>range</b> in <see cref="EstimatedTotalMin"/> /
/// <see cref="EstimatedTotalMax"/> and the cashier confirms the final price on arrival.
/// </para>
/// <para>
/// <b>Queue integration:</b> A Hangfire job creates a <see cref="QueueEntry"/>
/// ~15 minutes before the slot with <see cref="QueuePriority.Booked"/>.
/// When service starts, the link to <see cref="Transaction"/> is established.
/// </para>
/// </summary>
public sealed class Booking : IAuditableEntity
{
    private Booking() { } // EF Core

    public Booking(
        string tenantId,
        string branchId,
        string customerId,
        string connectUserId,
        string connectVehicleId,
        DateTime slotStart,
        DateTime slotEnd,
        int estimatedDurationMinutes)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        CustomerId = customerId;
        ConnectUserId = connectUserId;
        ConnectVehicleId = connectVehicleId;
        SlotStart = slotStart;
        SlotEnd = slotEnd;
        EstimatedDurationMinutes = estimatedDurationMinutes;
        Status = BookingStatus.Confirmed;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;

    /// <summary>The customer's global vehicle record — always set.</summary>
    public string ConnectVehicleId { get; set; } = string.Empty;

    /// <summary>
    /// The tenant's <see cref="Car"/> record. Nullable — set only if this vehicle
    /// has been classified at this tenant (return visit), or once the cashier
    /// classifies it at arrival.
    /// </summary>
    public string? CarId { get; set; }

    /// <summary>UTC slot start (Manila local time converted to UTC).</summary>
    public DateTime SlotStart { get; set; }

    /// <summary>UTC slot end.</summary>
    public DateTime SlotEnd { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    /// <summary>
    /// <c>true</c> → pricing is exact; <c>false</c> → pricing is a range
    /// (see <see cref="EstimatedTotalMin"/> / <see cref="EstimatedTotalMax"/>).
    /// </summary>
    public bool IsVehicleClassified { get; set; }

    /// <summary>
    /// Exact total when classified, midpoint estimate when not.
    /// Precision (10, 2).
    /// </summary>
    public decimal EstimatedTotal { get; set; }

    /// <summary>Low end of the price range — set only when not classified.</summary>
    public decimal? EstimatedTotalMin { get; set; }

    /// <summary>High end of the price range — set only when not classified.</summary>
    public decimal? EstimatedTotalMax { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    /// <summary>Created when the booking enters the physical queue (auto or on arrival).</summary>
    public string? QueueEntryId { get; set; }

    /// <summary>Created when service starts on the POS.</summary>
    public string? TransactionId { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ConnectUser ConnectUser { get; set; } = null!;
    public ConnectVehicle ConnectVehicle { get; set; } = null!;
    public Car? Car { get; set; }
    public QueueEntry? QueueEntry { get; set; }
    public Transaction? Transaction { get; set; }

    public ICollection<BookingService> Services { get; set; } = [];
}
