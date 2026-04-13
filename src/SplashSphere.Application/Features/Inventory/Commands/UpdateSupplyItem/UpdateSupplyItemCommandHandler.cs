using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateSupplyItem;

public sealed class UpdateSupplyItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSupplyItemCommand, Result>
{
    public async Task<Result> Handle(UpdateSupplyItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.SupplyItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
            return Result.Failure(Error.NotFound("SupplyItem", request.Id));

        item.Name = request.Name;
        item.Unit = request.Unit;
        item.CategoryId = request.CategoryId;
        item.Description = request.Description;
        item.ReorderLevel = request.ReorderLevel;
        item.IsActive = request.IsActive;

        return Result.Success();
    }
}
