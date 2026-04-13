using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.RegisterEquipment;

public sealed class RegisterEquipmentCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<RegisterEquipmentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        RegisterEquipmentCommand request,
        CancellationToken cancellationToken)
    {
        var equipment = new Equipment(tenantContext.TenantId, request.BranchId, request.Name)
        {
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            PurchaseDate = request.PurchaseDate,
            PurchaseCost = request.PurchaseCost,
            WarrantyExpiry = request.WarrantyExpiry,
            Location = request.Location,
            Notes = request.Notes,
        };

        db.Equipment.Add(equipment);
        return Result.Success(equipment.Id);
    }
}
