using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetMerchandiseById;

public sealed class GetMerchandiseByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMerchandiseByIdQuery, MerchandiseDto>
{
    public async Task<MerchandiseDto> Handle(
        GetMerchandiseByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Merchandise
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new MerchandiseDto(
                m.Id,
                m.Name,
                m.Sku,
                m.Description,
                m.CategoryId,
                m.Category != null ? m.Category.Name : null,
                m.Price,
                m.CostPrice,
                m.StockQuantity,
                m.LowStockThreshold,
                m.StockQuantity <= m.LowStockThreshold,
                m.IsActive,
                m.CreatedAt,
                m.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Merchandise '{request.Id}' was not found.");
    }
}
