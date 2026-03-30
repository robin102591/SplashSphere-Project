using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that captures entity changes and writes <see cref="AuditLog"/>
/// records within the same transaction. Registered as scoped (depends on <see cref="ITenantContext"/>).
/// </summary>
public sealed class AuditLogInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedTypes =
    [
        nameof(AuditLog),
        nameof(Notification),
        nameof(QueueEntry),
        nameof(Attendance),
        "ShiftPaymentSummary",
        "ShiftDenomination",
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAuditEntries(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !ExcludedTypes.Contains(e.Metadata.ClrType.Name))
            .ToList();

        if (entries.Count == 0) return;

        var tenantId = tenantContext.TenantId;
        if (string.IsNullOrEmpty(tenantId)) return; // No tenant = system operation, skip

        var userId = tenantContext.UserId;
        var auditSet = context.Set<AuditLog>();

        foreach (var entry in entries)
        {
            var entityType = entry.Metadata.ClrType.Name;
            var entityId = GetEntityId(entry);
            if (entityId is null) continue;

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => (AuditAction?)null,
            };

            if (action is null) continue;

            var changes = action.Value switch
            {
                AuditAction.Create => BuildCreateChanges(entry),
                AuditAction.Update => BuildUpdateChanges(entry),
                AuditAction.Delete => BuildDeleteChanges(entry),
                _ => null,
            };

            auditSet.Add(new AuditLog(
                ResolveTenantId(entry, tenantId),
                userId,
                action.Value,
                entityType,
                entityId,
                changes));
        }
    }

    private static string? GetEntityId(EntityEntry entry)
    {
        var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        return idProp?.CurrentValue?.ToString();
    }

    private static string ResolveTenantId(EntityEntry entry, string fallback)
    {
        var tenantProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "TenantId");
        var value = tenantProp?.CurrentValue?.ToString();
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    private static string? BuildCreateChanges(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.Name is "CreatedAt" or "UpdatedAt") continue;
            if (prop.CurrentValue is not null)
                dict[prop.Metadata.Name] = prop.CurrentValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, JsonOptions) : null;
    }

    private static string? BuildUpdateChanges(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.Name is "CreatedAt" or "UpdatedAt") continue;
            if (!prop.IsModified) continue;

            dict[prop.Metadata.Name] = new
            {
                old = prop.OriginalValue,
                @new = prop.CurrentValue,
            };
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, JsonOptions) : null;
    }

    private static string? BuildDeleteChanges(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (prop.CurrentValue is not null)
                dict[prop.Metadata.Name] = prop.CurrentValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict, JsonOptions) : null;
    }
}
