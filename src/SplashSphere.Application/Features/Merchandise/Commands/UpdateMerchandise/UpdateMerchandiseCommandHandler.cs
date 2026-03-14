using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.UpdateMerchandise;

public sealed class UpdateMerchandiseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateMerchandiseCommand, Result>
{
    public async Task<Result> Handle(
        UpdateMerchandiseCommand request,
        CancellationToken cancellationToken)
    {
        var merchandise = await context.Merchandise
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (merchandise is null)
            return Result.Failure(Error.NotFound("Merchandise", request.Id));

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            var categoryExists = await context.MerchandiseCategories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (!categoryExists)
                return Result.Failure(Error.Validation("Category ID is invalid."));
        }

        merchandise.Name               = request.Name;
        merchandise.Price              = request.Price;
        merchandise.CostPrice          = request.CostPrice;
        merchandise.LowStockThreshold  = request.LowStockThreshold;
        merchandise.CategoryId         = request.CategoryId;
        merchandise.Description        = request.Description;

        return Result.Success();
    }
}
