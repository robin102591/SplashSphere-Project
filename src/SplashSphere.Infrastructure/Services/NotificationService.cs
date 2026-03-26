using Microsoft.AspNetCore.SignalR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Infrastructure.Hubs;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Creates a persistent <see cref="Notification"/> record and broadcasts a
/// <c>NotificationReceived</c> SignalR event to the tenant group.
/// <para>
/// Accepts an explicit <c>tenantId</c> parameter (not from <c>TenantContext</c>)
/// so Hangfire jobs running cross-tenant can create notifications correctly.
/// Global query filters only affect reads, so inserts with explicit TenantId work.
/// </para>
/// </summary>
public sealed class NotificationService(
    IApplicationDbContext db,
    IHubContext<SplashSphereHub> hub) : INotificationService
{
    public async Task CreateAsync(
        string tenantId,
        NotificationType type,
        NotificationCategory category,
        string title,
        string message,
        string? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification(
            tenantId, type, category, title, message, referenceId, referenceType);

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.TenantGroup(tenantId))
            .SendAsync("NotificationReceived", new NotificationReceivedPayload(
                notification.Id,
                (int)notification.Type,
                (int)notification.Category,
                notification.Title,
                notification.Message,
                notification.ReferenceId,
                notification.ReferenceType,
                notification.CreatedAt),
                cancellationToken);
    }
}
