using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionSummaryDto>>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<PagedResult<TransactionSummaryDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve date range → UTC boundaries (Manila calendar date → UTC window)
        DateTime? fromUtc = null;
        DateTime? toUtc   = null;

        if (request.DateFrom.HasValue)
            fromUtc = request.DateFrom.Value.ToDateTime(TimeOnly.MinValue) - ManilaOffset;

        if (request.DateTo.HasValue)
            toUtc = request.DateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset;

        var query = context.Transactions
            .AsNoTracking()
            .Where(t => t.BranchId == request.BranchId);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (fromUtc.HasValue)
            query = query.Where(t => t.CreatedAt >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(t => t.CreatedAt < toUtc.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t =>
                t.TransactionNumber.ToLower().Contains(search) ||
                t.Car.PlateNumber.ToLower().Contains(search) ||
                (t.Customer != null && (
                    t.Customer.FirstName.ToLower().Contains(search) ||
                    t.Customer.LastName.ToLower().Contains(search))));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionSummaryDto(
                t.Id,
                t.TransactionNumber,
                t.BranchId,
                t.Branch.Name,
                t.CarId,
                t.Car.PlateNumber,
                t.Car.VehicleType.Name,
                t.Car.Size.Name,
                t.CustomerId,
                t.Customer != null ? t.Customer.FirstName + " " + t.Customer.LastName : null,
                t.Status,
                t.TotalAmount,
                t.DiscountAmount,
                t.TaxAmount,
                t.FinalAmount,
                t.Cashier.FirstName + " " + t.Cashier.LastName,
                t.QueueEntry != null ? t.QueueEntry.Id : null,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<TransactionSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
