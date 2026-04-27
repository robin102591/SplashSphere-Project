namespace SplashSphere.Domain.Entities;

/// <summary>
/// Global vehicle manufacturer catalogue used by the Connect app's vehicle picker
/// (e.g. Toyota, Honda, Mitsubishi). <b>Not tenant-scoped</b> — shared across all
/// SplashSphere customers. Distinct from the per-tenant <see cref="Make"/> entity,
/// which tenants manage for their own POS catalogue.
/// <para>
/// Seeded with common Philippine vehicles. Tenants can request additions as needed.
/// </para>
/// </summary>
public sealed class GlobalMake
{
    private GlobalMake() { } // EF Core

    public GlobalMake(string name, int displayOrder = 0)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;

    /// <summary>Make name (e.g. "Toyota"). Globally unique.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sort order for the UI picker. Lower = earlier.</summary>
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ICollection<GlobalModel> Models { get; set; } = [];
}
