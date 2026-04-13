using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateEquipmentStatus;

public sealed class UpdateEquipmentStatusCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateEquipmentStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateEquipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var equipment = await db.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (equipment is null)
            return Result.Failure(Error.NotFound("Equipment", request.Id));

        equipment.Status = request.Status;
        return Result.Success();
    }
}
