namespace SplashSphere.Domain.Entities;

/// <summary>
/// An authenticated user of the system, backed by a Clerk user record.
/// <see cref="TenantId"/> is null for newly signed-up users who have not yet
/// completed onboarding. After onboarding (or invitation acceptance) it is
/// populated with the Clerk Organization ID.
/// </summary>
public sealed class User : IAuditableEntity
{
    private User() { } // EF Core

    public User(string clerkUserId, string email, string firstName, string lastName)
    {
        Id = Guid.NewGuid().ToString();
        ClerkUserId = clerkUserId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Clerk user ID (<c>sub</c> claim). Unique across the platform.
    /// Used by <c>TenantResolutionMiddleware</c> to look up the internal user.
    /// </summary>
    public string ClerkUserId { get; set; } = string.Empty;

    /// <summary>
    /// Clerk Organization ID. Null until the user completes onboarding
    /// or accepts a branch invitation.
    /// </summary>
    public string? TenantId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Org-level role from Clerk (<c>org_role</c> claim), e.g. "org:admin", "org:member".
    /// </summary>
    public string? Role { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// BCrypt hash of the user's 6-digit POS lock PIN.
    /// Null if no PIN has been set — POS lock is not enforced for this user.
    /// </summary>
    public string? PinHash { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant? Tenant { get; set; }

    /// <summary>Shifts this user has operated as cashier.</summary>
    public ICollection<CashierShift> CashierShifts { get; set; } = [];

    /// <summary>Shifts this user has reviewed as manager.</summary>
    public ICollection<CashierShift> ReviewedShifts { get; set; } = [];
}
