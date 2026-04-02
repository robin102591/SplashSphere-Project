using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Daily Hangfire job (00:30 PHT) that auto-generates expense records for
/// recurring expenses based on their <see cref="ExpenseFrequency"/>.
/// <para>
/// For each recurring expense, checks whether a new record is due:
/// <list type="bullet">
///   <item><c>Daily</c> — generates one every day.</item>
///   <item><c>Weekly</c> — generates one if the last expense date is 7+ days ago.</item>
///   <item><c>Monthly</c> — generates one if the last expense date is in a previous month.</item>
/// </list>
/// New records copy the original's amount, category, branch, vendor, and description.
/// </para>
/// </summary>
public sealed class ExpenseJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpenseJobService> logger)
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    [AutomaticRetry(Attempts = 2)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task GenerateRecurringExpensesAsync(CancellationToken ct = default)
    {
        logger.LogInformation("ExpenseJob: Checking recurring expenses across all tenants.");

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var todayManila = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);

        // Load all recurring, non-deleted expenses across all tenants
        var recurring = await db.Expenses
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(e => e.IsRecurring && !e.IsDeleted && e.Frequency != ExpenseFrequency.OneTime)
            .Select(e => new
            {
                e.Id,
                e.TenantId,
                e.BranchId,
                e.RecordedById,
                e.CategoryId,
                e.Amount,
                e.Description,
                e.Vendor,
                e.Frequency,
                LastDate = DateOnly.FromDateTime(e.ExpenseDate),
            })
            .ToListAsync(ct);

        if (recurring.Count == 0)
        {
            logger.LogInformation("ExpenseJob: No recurring expenses found.");
            return;
        }

        var created = 0;

        foreach (var src in recurring)
        {
            if (!IsDue(src.Frequency, src.LastDate, todayManila))
                continue;

            // Check if we already generated one for today (idempotency)
            var alreadyExists = await db.Expenses
                .IgnoreQueryFilters()
                .AnyAsync(e =>
                    e.TenantId == src.TenantId &&
                    e.CategoryId == src.CategoryId &&
                    e.BranchId == src.BranchId &&
                    e.Amount == src.Amount &&
                    e.Description == src.Description &&
                    !e.IsDeleted &&
                    e.ExpenseDate.Date == todayManila.ToDateTime(TimeOnly.MinValue).Date,
                    ct);

            if (alreadyExists)
                continue;

            var expense = new Expense(
                src.TenantId,
                src.BranchId,
                src.RecordedById,
                src.CategoryId,
                src.Amount,
                src.Description,
                todayManila.ToDateTime(TimeOnly.MinValue))
            {
                Vendor = src.Vendor,
                Frequency = src.Frequency,
                IsRecurring = true,
            };

            db.Expenses.Add(expense);
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "ExpenseJob: Generated {Count} recurring expense(s) for {Date}.",
            created, todayManila);
    }

    private static bool IsDue(ExpenseFrequency frequency, DateOnly lastDate, DateOnly today)
    {
        return frequency switch
        {
            ExpenseFrequency.Daily => lastDate < today,
            ExpenseFrequency.Weekly => lastDate.AddDays(7) <= today,
            ExpenseFrequency.Monthly => lastDate.AddMonths(1) <= today,
            _ => false,
        };
    }
}
