using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Hangfire jobs for inventory management:
/// <list type="bullet">
///   <item>Low-stock alerts — every 6 hours — scans merchandise and supply items, publishes
///     <see cref="LowStockAlertEvent"/> for items at or below threshold.</item>
///   <item>Equipment maintenance — daily at midnight UTC (8 AM PHT) — flags overdue equipment
///     as <see cref="EquipmentStatus.NeedsMaintenance"/>.</item>
/// </list>
/// </summary>
public sealed class InventoryJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<InventoryJobService> logger)
{
    /// <summary>
    /// Checks all supply items and merchandise for low stock across all tenants.
    /// Publishes <see cref="LowStockAlertEvent"/> for each low-stock merchandise item.
    /// Also logs warnings for low-stock supply items.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CheckLowStockAlertsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("InventoryJob: Checking low-stock items across all tenants.");

        using var scope = scopeFactory.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        // ── Merchandise ──────────────────────────────────────────────────────
        var lowMerch = await db.Merchandise
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

        foreach (var item in lowMerch)
        {
            publisher.Enqueue(new LowStockAlertEvent(
                item.Id,
                item.TenantId,
                item.Name,
                item.Sku,
                item.StockQuantity,
                item.LowStockThreshold));

            logger.LogInformation(
                "InventoryJob: Low-stock alert for merchandise [{Sku}] {Name} — {Stock}/{Threshold} (tenant {TenantId}).",
                item.Sku, item.Name, item.StockQuantity, item.LowStockThreshold, item.TenantId);
        }

        if (lowMerch.Count > 0)
            await publisher.FlushAsync(ct);

        // ── Supply items ─────────────────────────────────────────────────────
        var lowSupplies = await db.SupplyItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.IsActive && s.ReorderLevel.HasValue && s.CurrentStock <= s.ReorderLevel.Value)
            .Select(s => new
            {
                s.TenantId,
                s.Name,
                s.CurrentStock,
                s.ReorderLevel,
                BranchName = s.Branch.Name,
            })
            .ToListAsync(ct);

        foreach (var item in lowSupplies)
        {
            logger.LogWarning(
                "InventoryJob: Low supply stock — {Item} at {Branch} ({Tenant}) — {Stock}/{Reorder}.",
                item.Name, item.BranchName, item.TenantId, item.CurrentStock, item.ReorderLevel);
        }

        logger.LogInformation(
            "InventoryJob: Low stock check complete — {MerchAlerts} merchandise, {SupplyAlerts} supplies.",
            lowMerch.Count, lowSupplies.Count);
    }

    /// <summary>
    /// Checks equipment maintenance schedules across all tenants. Sets status to
    /// <see cref="EquipmentStatus.NeedsMaintenance"/> for operational equipment
    /// whose latest maintenance log has an overdue NextDueDate.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CheckEquipmentMaintenanceAsync(CancellationToken ct = default)
    {
        logger.LogInformation("InventoryJob: Checking equipment maintenance schedules.");

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTime.UtcNow;

        // Find operational equipment where the most recent maintenance log has NextDueDate <= now
        var overdueEquipment = await db.Equipment
            .IgnoreQueryFilters()
            .Where(e => e.IsActive && e.Status == EquipmentStatus.Operational)
            .Where(e => e.MaintenanceLogs
                .OrderByDescending(ml => ml.PerformedDate)
                .Select(ml => ml.NextDueDate)
                .FirstOrDefault() <= now)
            .ToListAsync(ct);

        foreach (var eq in overdueEquipment)
        {
            eq.Status = EquipmentStatus.NeedsMaintenance;
            logger.LogWarning(
                "InventoryJob: Equipment maintenance overdue — {Equipment} (tenant {TenantId}).",
                eq.Name, eq.TenantId);
        }

        if (overdueEquipment.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "InventoryJob: Equipment maintenance check complete — {Count} item(s) flagged.",
            overdueEquipment.Count);
    }
}
