using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Common.Interfaces;

public interface INotificationService
{
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
