namespace SplashSphere.Domain.Entities;

/// <summary>
/// A physical car wash location owned by a <see cref="Tenant"/>.
/// <see cref="Code"/> is the short prefix used in transaction numbers:
/// <c>{Code}-{YYYYMMDD}-{Sequence}</c>.
/// </summary>
public sealed class Branch : IAuditableEntity
{
    private Branch() { } // EF Core

    public Branch(string tenantId, string name, string code, string address, string contactNumber)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        Code = code.ToUpperInvariant();
        Address = address;
        ContactNumber = contactNumber;
    }

    public string Id { get; set; } = string.Empty;

    /// <summary>Tenant discriminator — Clerk Organization ID.</summary>
    public string TenantId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Short uppercase code for this branch (e.g. "MKT", "BGC").
    /// Used as prefix in transaction numbers.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<QueueEntry> QueueEntries { get; set; } = [];
}
