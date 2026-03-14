using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Daily Hangfire job (08:00 PHT) that scans every active merchandise item across
/// all tenants and publishes a <see cref="LowStockAlertEvent"/> for any item whose
/// <c>StockQuantity &lt;= LowStockThreshold</c>.
/// <para>
/// The event is consumed by the SignalR notification pipeline which broadcasts an
/// in-app alert to the tenant's admin dashboard group.
/// </para>
/// </summary>
public sealed class InventoryJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<InventoryJobService> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CheckLowStockAlertsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("InventoryJob: Checking low-stock merchandise across all tenants.");

        using var scope = scopeFactory.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        // Cross-tenant scan — IgnoreQueryFilters bypasses the TenantId global filter.
        var lowStock = await db.Merchandise
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.IsActive && m.StockQuantity <= m.LowStockThreshold)
            .Select(m => new
            {
                m.Id,
                m.TenantId,
                m.Name,
                m.Sku,
                m.StockQuantity,
                m.LowStockThreshold,
            })
            .ToListAsync(ct);

        if (lowStock.Count == 0)
        {
            logger.LogInformation("InventoryJob: No low-stock items found.");
            return;
        }

        logger.LogInformation(
            "InventoryJob: Found {Count} low-stock item(s). Publishing alerts.", lowStock.Count);

        foreach (var item in lowStock)
        {
            await publisher.PublishAsync(new LowStockAlertEvent(
                item.Id,
                item.TenantId,
                item.Name,
                item.Sku,
                item.StockQuantity,
                item.LowStockThreshold), ct);

            logger.LogInformation(
                "InventoryJob: Low-stock alert published for [{Sku}] {Name} — {Stock}/{Threshold} (tenant {TenantId}).",
                item.Sku, item.Name, item.StockQuantity, item.LowStockThreshold, item.TenantId);
        }
    }
}
