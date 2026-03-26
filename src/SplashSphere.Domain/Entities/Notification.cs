namespace SplashSphere.Domain.Entities;

public sealed class Notification : IAuditableEntity
{
    private Notification() { } // EF Core

    public Notification(
        string tenantId,
        NotificationType type,
        NotificationCategory category,
        string title,
        string message,
        string? referenceId = null,
        string? referenceType = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Type = type;
        Category = category;
        Title = title;
        Message = message;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
