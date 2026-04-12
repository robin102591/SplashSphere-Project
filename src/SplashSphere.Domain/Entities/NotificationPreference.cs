namespace SplashSphere.Domain.Entities;

public sealed class NotificationPreference
{
    private NotificationPreference() { } // EF Core

    public NotificationPreference(
        string tenantId,
        string userId,
        NotificationType notificationType)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        UserId = userId;
        NotificationType = notificationType;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public bool SmsEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}
