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
        string? referenceType = null,
        NotificationSeverity severity = NotificationSeverity.Info,
        string? recipientUserId = null,
        string? actionUrl = null,
        string? actionLabel = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Type = type;
        Category = category;
        Title = title;
        Message = message;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        Severity = severity;
        RecipientUserId = recipientUserId;
        ActionUrl = actionUrl;
        ActionLabel = actionLabel;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public NotificationSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? RecipientUserId { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionLabel { get; set; }
    public string? Metadata { get; set; }
    public bool IsRead { get; set; }
    public bool InAppDelivered { get; set; }
    public bool SmsDelivered { get; set; }
    public bool EmailDelivered { get; set; }
    public bool SmsSkipped { get; set; }
    public bool EmailSkipped { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
