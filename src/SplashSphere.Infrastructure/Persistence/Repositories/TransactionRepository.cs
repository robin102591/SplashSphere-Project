using Microsoft.EntityFrameworkCore;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete repository for the <see cref="Transaction"/> aggregate.
/// Extends the generic base with full-graph eager loading, daily sequence generation,
/// and branch-date filtering used by the POS daily summary.
/// </summary>
public sealed class TransactionRepository(ApplicationDbContext context)
    : TenantAwareRepository<Transaction>(context), ITransactionRepository
{
    public async Task<Transaction?> GetWithDetailsAsync(
        string id,
        CancellationToken cancellationToken = default) =>
        await Context.Transactions
            .AsNoTracking()
            .Include(t => t.Branch)
            .Include(t => t.Cashier)
            .Include(t => t.Car)
                .ThenInclude(c => c.VehicleType)
            .Include(t => t.Car)
                .ThenInclude(c => c.Size)
            .Include(t => t.Customer)
            .Include(t => t.Services)
                .ThenInclude(ts => ts.Service)
            .Include(t => t.Services)
                .ThenInclude(ts => ts.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
            .Include(t => t.Packages)
                .ThenInclude(tp => tp.Package)
            .Include(t => t.Packages)
                .ThenInclude(tp => tp.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
            .Include(t => t.Merchandise)
                .ThenInclude(tm => tm.Merchandise)
            .Include(t => t.Employees)
                .ThenInclude(te => te.Employee)
            .Include(t => t.Payments)
            .Include(t => t.QueueEntry)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<int> GetNextDailySequenceAsync(
        string branchId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Count all transactions for this branch on the given calendar date.
        // The date window is expressed in UTC to match how CreatedAt is stored.
        // The (TenantId, TransactionNumber) unique index provides the final conflict
        // guard — handlers catch unique violations and retry with a fresh call.
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var count = await Context.Transactions
            .AsNoTracking()
            .CountAsync(
                t => t.BranchId == branchId
                  && t.CreatedAt >= dayStart
                  && t.CreatedAt < dayEnd,
                cancellationToken);

        return count + 1;
    }

    public async Task<IReadOnlyList<Transaction>> GetByBranchAndDateAsync(
        string branchId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        return await Context.Transactions
            .AsNoTracking()
            .Where(t => t.BranchId == branchId
                     && t.CreatedAt >= dayStart
                     && t.CreatedAt < dayEnd)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
