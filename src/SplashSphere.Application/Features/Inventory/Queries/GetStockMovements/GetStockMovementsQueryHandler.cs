using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetStockMovements;

public sealed class GetStockMovementsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public async Task<PagedResult<StockMovementDto>> Handle(
        GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = db.StockMovements.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SupplyItemId))
            query = query.Where(m => m.SupplyItemId == request.SupplyItemId);

        if (!string.IsNullOrWhiteSpace(request.MerchandiseId))
            query = query.Where(m => m.MerchandiseId == request.MerchandiseId);

        if (request.Type.HasValue)
            query = query.Where(m => m.Type == request.Type.Value);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(m => m.BranchId == request.BranchId);

        if (request.From.HasValue)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(m => m.MovementDate >= fromUtc);
        }

        if (request.To.HasValue)
        {
            var toUtc = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(m => m.MovementDate < toUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new StockMovementDto(
                m.Id,
                m.Branch.Name,
                m.SupplyItem != null ? m.SupplyItem.Name
                    : m.Merchandise != null ? m.Merchandise.Name : "",
                m.Type.ToString(),
                m.Quantity,
                m.UnitCost,
                m.TotalCost,
                m.Reference,
                m.Notes,
                m.PerformedByUserId,
                m.MovementDate))
            .ToListAsync(cancellationToken);

        return PagedResult<StockMovementDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
