using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Queries.GetBillingHistory;

public sealed class GetBillingHistoryQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetBillingHistoryQuery, PagedResult<BillingRecordDto>>
{
    public async Task<PagedResult<BillingRecordDto>> Handle(
        GetBillingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.BillingRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.TenantId == tenantContext.TenantId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.BillingDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BillingRecordDto(
                b.Id,
                b.Amount,
                b.Currency,
                b.Type,
                b.Status,
                b.PaymentMethod,
                b.InvoiceNumber,
                b.BillingDate,
                b.PaidDate,
                b.Notes))
            .ToListAsync(cancellationToken);

        return PagedResult<BillingRecordDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
