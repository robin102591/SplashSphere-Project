using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.CreateMerchandiseCategory;

public sealed class CreateMerchandiseCategoryCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateMerchandiseCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateMerchandiseCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.MerchandiseCategories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(
                Error.Conflict($"Merchandise category '{request.Name}' already exists."));

        var category = new MerchandiseCategory(tenantContext.TenantId, request.Name, request.Description);
        context.MerchandiseCategories.Add(category);

        return Result.Success(category.Id);
    }
}
