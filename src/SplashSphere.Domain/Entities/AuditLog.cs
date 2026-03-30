using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Immutable record of a data change — who did what, when, on which entity.
/// Created automatically by <c>AuditLogInterceptor</c> during SaveChanges.
/// </summary>
public sealed class AuditLog
{
    public AuditLog(
        string tenantId,
        string? userId,
        AuditAction action,
        string entityType,
        string entityId,
        string? changes)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Changes = changes;
        Timestamp = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    /// <summary>JSON diff of changed properties. Null for deletes with no snapshot.</summary>
    public string? Changes { get; set; }
    public DateTime Timestamp { get; set; }
}
