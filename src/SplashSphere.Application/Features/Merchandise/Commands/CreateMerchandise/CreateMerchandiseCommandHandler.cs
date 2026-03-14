using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.CreateMerchandise;

public sealed class CreateMerchandiseCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateMerchandiseCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateMerchandiseCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();

        var skuExists = await context.Merchandise
            .AnyAsync(m => m.Sku == normalizedSku, cancellationToken);

        if (skuExists)
            return Result.Failure<string>(Error.Conflict($"Merchandise with SKU '{normalizedSku}' already exists."));

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            var categoryExists = await context.MerchandiseCategories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
                return Result.Failure<string>(Error.Validation("Category ID is invalid."));
        }

        var merchandise = new Domain.Entities.Merchandise(
            tenantContext.TenantId,
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity,
            request.LowStockThreshold,
            request.CategoryId,
            request.Description,
            request.CostPrice);

        context.Merchandise.Add(merchandise);

        return Result.Success(merchandise.Id);
    }
}
