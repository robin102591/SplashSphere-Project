using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="LowStockAlertEvent"/> raised by the daily Hangfire stock check.
/// Persists a <c>Notification</c> record and broadcasts a <c>LowStockAlert</c>
/// SignalR event to the tenant group so the admin dashboard can show alerts.
/// </summary>
public sealed class LowStockAlertNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    INotificationService notificationService)
    : INotificationHandler<DomainEventNotification<LowStockAlertEvent>>
{
    public async Task Handle(
        DomainEventNotification<LowStockAlertEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Persist notification for low-stock item.
        await notificationService.CreateAsync(
            e.TenantId,
            NotificationType.LowStockAlert,
            NotificationCategory.Inventory,
            "Low Stock Alert",
            $"{e.MerchandiseName} ({e.Sku}) is low — {e.CurrentStock} remaining (threshold: {e.LowStockThreshold}).",
            e.MerchandiseId,
            "Merchandise",
            cancellationToken);

        // Broadcast real-time alert to admin dashboard.
        await hub.Clients
            .Group(SplashSphereHub.TenantGroup(e.TenantId))
            .SendAsync("LowStockAlert", new LowStockAlertPayload(
                e.MerchandiseId,
                e.MerchandiseName,
                e.Sku,
                e.CurrentStock,
                e.LowStockThreshold),
                cancellationToken);
    }
}
