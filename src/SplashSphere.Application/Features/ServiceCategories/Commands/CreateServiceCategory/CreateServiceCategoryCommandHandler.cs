using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public sealed class CreateServiceCategoryCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateServiceCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateServiceCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.ServiceCategories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(
                Error.Conflict($"Service category '{request.Name}' already exists."));

        var category = new ServiceCategory(tenantContext.TenantId, request.Name, request.Description);
        context.ServiceCategories.Add(category);

        return Result.Success(category.Id);
    }
}
