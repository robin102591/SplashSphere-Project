using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Search.Queries.GlobalSearch;

public sealed class GlobalSearchQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GlobalSearchQuery, GlobalSearchResultDto>
{
    public async Task<GlobalSearchResultDto> Handle(
        GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var q = request.Q.Trim().ToLower();
        var limit = Math.Clamp(request.Limit, 1, 10);

        // Case-insensitive search using ToLower().Contains() which Npgsql
        // translates to efficient LOWER(col) LIKE '%term%' on PostgreSQL.
        // All DbSets already have tenant global query filters.

        // EF Core DbContext is not thread-safe, so queries run sequentially.
        // Each query is lightweight (Take ≤ 10, AsNoTracking) so this is fast.
        var customers = await context.Customers
            .AsNoTracking()
            .Where(c => (c.FirstName + " " + c.LastName).ToLower().Contains(q)
                      || (c.Email != null && c.Email.ToLower().Contains(q))
                      || (c.ContactNumber != null && c.ContactNumber.ToLower().Contains(q)))
            .OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
            .Take(limit)
            .Select(c => new SearchHitDto(
                c.Id,
                c.FirstName + " " + c.LastName,
                c.Email ?? c.ContactNumber,
                "customer"))
            .ToListAsync(cancellationToken);

        var employees = await context.Employees
            .AsNoTracking()
            .Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(q)
                      || (e.Email != null && e.Email.ToLower().Contains(q)))
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
            .Take(limit)
            .Select(e => new SearchHitDto(
                e.Id,
                e.FirstName + " " + e.LastName,
                e.Email,
                "employee"))
            .ToListAsync(cancellationToken);

        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionNumber.ToLower().Contains(q))
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new SearchHitDto(
                t.Id,
                t.TransactionNumber,
                t.Status.ToString(),
                "transaction"))
            .ToListAsync(cancellationToken);

        var vehicles = await context.Cars
            .AsNoTracking()
            .Where(c => c.PlateNumber.ToLower().Contains(q)
                      || (c.Color != null && c.Color.ToLower().Contains(q)))
            .OrderBy(c => c.PlateNumber)
            .Take(limit)
            .Select(c => new SearchHitDto(
                c.Id,
                c.PlateNumber,
                c.Color,
                "vehicle"))
            .ToListAsync(cancellationToken);

        var services = await context.Services
            .AsNoTracking()
            .Where(s => s.Name.ToLower().Contains(q))
            .OrderBy(s => s.Name)
            .Take(limit)
            .Select(s => new SearchHitDto(
                s.Id,
                s.Name,
                s.Description,
                "service"))
            .ToListAsync(cancellationToken);

        var merchandise = await context.Merchandise
            .AsNoTracking()
            .Where(m => m.Name.ToLower().Contains(q)
                      || m.Sku.ToLower().Contains(q))
            .OrderBy(m => m.Name)
            .Take(limit)
            .Select(m => new SearchHitDto(
                m.Id,
                m.Name,
                m.Sku,
                "merchandise"))
            .ToListAsync(cancellationToken);

        return new GlobalSearchResultDto(
            Customers: customers,
            Employees: employees,
            Transactions: transactions,
            Vehicles: vehicles,
            Services: services,
            Merchandise: merchandise);
    }
}
