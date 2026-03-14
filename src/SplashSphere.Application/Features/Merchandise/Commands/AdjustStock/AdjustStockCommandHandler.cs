using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AdjustStockCommand, Result>
{
    public async Task<Result> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        var merchandise = await context.Merchandise
            .FirstOrDefaultAsync(m => m.Id == request.MerchandiseId, cancellationToken);

        if (merchandise is null)
            return Result.Failure(Error.NotFound("Merchandise", request.MerchandiseId));

        var newQuantity = merchandise.StockQuantity + request.Adjustment;

        if (newQuantity < 0)
            return Result.Failure(Error.Validation(
                $"Adjustment of {request.Adjustment} would result in negative stock " +
                $"(current: {merchandise.StockQuantity}). Reduce the adjustment amount."));

        merchandise.StockQuantity = newQuantity;

        return Result.Success();
    }
}
