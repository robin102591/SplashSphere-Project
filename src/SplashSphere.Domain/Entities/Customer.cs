namespace SplashSphere.Domain.Entities;

/// <summary>
/// A registered car wash customer belonging to a tenant.
/// Customers are optional — a <see cref="Car"/> can exist without a linked customer
/// (walk-in vehicle), but associating one enables history tracking and loyalty features.
/// </summary>
public sealed class Customer : IAuditableEntity
{
    private Customer() { } // EF Core

    public Customer(string tenantId, string firstName, string lastName,
        string? email = null, string? contactNumber = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        ContactNumber = contactNumber;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Car> Cars { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<QueueEntry> QueueEntries { get; set; } = [];
}
