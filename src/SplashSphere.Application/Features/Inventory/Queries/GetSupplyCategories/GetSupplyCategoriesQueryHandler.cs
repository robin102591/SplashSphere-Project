using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyCategories;

public sealed class GetSupplyCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSupplyCategoriesQuery, IReadOnlyList<SupplyCategoryDto>>
{
    public async Task<IReadOnlyList<SupplyCategoryDto>> Handle(
        GetSupplyCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await db.SupplyCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SupplyCategoryDto(c.Id, c.Name, c.Description, c.IsActive))
            .ToListAsync(cancellationToken);
    }
}
