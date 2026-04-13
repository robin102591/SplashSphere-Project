using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreateSupplierCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = new Supplier(tenantContext.TenantId, request.Name)
        {
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
        };

        db.Suppliers.Add(supplier);
        return Result.Success(supplier.Id);
    }
}
