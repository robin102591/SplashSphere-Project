namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised by the <c>CheckLowStockAlerts</c> Hangfire job (daily 08:00 PHT) when a
/// merchandise item's <c>StockQuantity</c> is at or below its <c>LowStockThreshold</c>.
/// Consumed by: admin notification (in-app alert or email), SignalR broadcast to
/// the tenant's dashboard group.
/// </summary>
public sealed record LowStockAlertEvent(
    string MerchandiseId,
    string TenantId,
    string MerchandiseName,
    string Sku,
    int CurrentStock,
    int LowStockThreshold
) : DomainEventBase;
