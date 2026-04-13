using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSuppliers;

public sealed class GetSuppliersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSuppliersQuery, IReadOnlyList<SupplierDto>>
{
    public async Task<IReadOnlyList<SupplierDto>> Handle(
        GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        return await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto(
                s.Id, s.Name, s.ContactPerson, s.Phone, s.Email, s.Address, s.IsActive))
            .ToListAsync(cancellationToken);
    }
}
