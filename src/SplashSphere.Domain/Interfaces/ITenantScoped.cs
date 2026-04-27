namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Marker interface declaring that an entity is scoped to a single tenant and
/// must be subject to the standard global query filter
/// <c>e =&gt; e.TenantId == tenantContext.TenantId</c>.
/// <para>
/// <c>ApplicationDbContext.OnModelCreating</c> scans the model for every CLR
/// type implementing this interface and registers the filter automatically.
/// Adding the marker is the only step required to make a new tenant-scoped
/// entity safely participate in the multi-tenancy isolation guarantees —
/// forgetting to add it is the only way to leak rows across tenants, which
/// makes the type system the single source of truth.
/// </para>
/// <para>
/// Implementing types must expose a <c>string TenantId</c> column (the EF Core
/// metadata layer is what the dynamic filter binds to, so the property name
/// must match exactly). Entities with non-standard scoping (different column
/// name, additional predicates such as soft-delete) keep a hand-written
/// <c>HasQueryFilter</c> registration in <c>OnModelCreating</c> instead.
/// </para>
/// </summary>
public interface ITenantScoped
{
}
