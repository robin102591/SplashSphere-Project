using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.DeleteSupplyItem;

public sealed class DeleteSupplyItemCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSupplyItemCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplyItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.SupplyItems
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
            return Result.Failure(Error.NotFound("SupplyItem", request.Id));

        item.IsActive = false;
        return Result.Success();
    }
}
