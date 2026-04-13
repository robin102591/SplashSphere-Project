using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CancelPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po is null)
            return Result.Failure(Error.NotFound("PurchaseOrder", request.Id));

        if (po.Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.Sent))
            return Result.Failure(Error.Validation("PurchaseOrder.InvalidStatus",
                "Only draft or sent purchase orders can be cancelled."));

        po.Status = PurchaseOrderStatus.Cancelled;
        return Result.Success();
    }
}
