using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Unified notification service. Creates persistent notifications and routes
/// delivery across channels (in-app, SMS, email) based on notification type
/// configuration and user preferences.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification through the unified routing pipeline.
    /// Determines channels from <see cref="NotificationTypeConfig"/> and user preferences.
    /// </summary>
    Task SendAsync(SendNotificationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Backward-compatible overload used by existing event handlers.
    /// Routes through the same unified pipeline.
    /// </summary>
    Task CreateAsync(
        string tenantId,
        NotificationType type,
        NotificationCategory category,
        string title,
        string message,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to send a notification through the unified pipeline.
/// </summary>
public sealed record SendNotificationRequest
{
    public required string TenantId { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public string? ActionUrl { get; init; }
    public string? ActionLabel { get; init; }
    public string? Metadata { get; init; }

    /// <summary>Target a specific user. Null = broadcast to all tenant users.</summary>
    public string? RecipientUserId { get; init; }

    /// <summary>For customer-facing SMS. Overrides user phone lookup.</summary>
    public string? RecipientPhone { get; init; }

    /// <summary>For customer-facing or billing email. Overrides user email lookup.</summary>
    public string? RecipientEmail { get; init; }
}
