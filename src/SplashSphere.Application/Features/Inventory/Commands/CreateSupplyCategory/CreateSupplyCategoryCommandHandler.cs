using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplyCategory;

public sealed class CreateSupplyCategoryCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreateSupplyCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateSupplyCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = new SupplyCategory(tenantContext.TenantId, request.Name, request.Description);
        db.SupplyCategories.Add(category);
        return Result.Success(category.Id);
    }
}
