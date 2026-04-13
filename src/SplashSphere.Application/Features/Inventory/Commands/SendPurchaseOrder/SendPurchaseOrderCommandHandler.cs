using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.SendPurchaseOrder;

public sealed class SendPurchaseOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<SendPurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(SendPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po is null)
            return Result.Failure(Error.NotFound("PurchaseOrder", request.Id));

        if (po.Status != PurchaseOrderStatus.Draft)
            return Result.Failure(Error.Validation("PurchaseOrder.NotDraft",
                "Only draft purchase orders can be sent."));

        po.Status = PurchaseOrderStatus.Sent;
        return Result.Success();
    }
}
