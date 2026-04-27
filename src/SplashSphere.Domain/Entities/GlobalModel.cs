namespace SplashSphere.Domain.Entities;

/// <summary>
/// Global vehicle model catalogue owned by a <see cref="GlobalMake"/>
/// (e.g. Toyota Vios). <b>Not tenant-scoped.</b>
/// </summary>
public sealed class GlobalModel
{
    private GlobalModel() { } // EF Core

    public GlobalModel(string globalMakeId, string name, int displayOrder = 0)
    {
        Id = Guid.NewGuid().ToString();
        GlobalMakeId = globalMakeId;
        Name = name;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string GlobalMakeId { get; set; } = string.Empty;

    /// <summary>Model name (e.g. "Vios"). Unique within the parent make.</summary>
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public GlobalMake Make { get; set; } = null!;
}
