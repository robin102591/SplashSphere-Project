using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateEquipment;

public sealed class UpdateEquipmentCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateEquipmentCommand, Result>
{
    public async Task<Result> Handle(UpdateEquipmentCommand request, CancellationToken cancellationToken)
    {
        var equipment = await db.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (equipment is null)
            return Result.Failure(Error.NotFound("Equipment", request.Id));

        equipment.Name = request.Name;
        equipment.Brand = request.Brand;
        equipment.Model = request.Model;
        equipment.SerialNumber = request.SerialNumber;
        equipment.PurchaseDate = request.PurchaseDate;
        equipment.PurchaseCost = request.PurchaseCost;
        equipment.WarrantyExpiry = request.WarrantyExpiry;
        equipment.Location = request.Location;
        equipment.Notes = request.Notes;
        equipment.IsActive = request.IsActive;

        return Result.Success();
    }
}
