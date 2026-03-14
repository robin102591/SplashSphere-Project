using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.CreateService;

public sealed class CreateServiceCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateServiceCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateServiceCommand request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await context.ServiceCategories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return Result.Failure<string>(Error.NotFound("ServiceCategory", request.CategoryId));

        var nameExists = await context.Services
            .AnyAsync(s => s.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(
                Error.Conflict($"Service '{request.Name}' already exists."));

        var service = new Service(
            tenantContext.TenantId,
            request.CategoryId,
            request.Name,
            request.BasePrice,
            request.Description);

        context.Services.Add(service);

        return Result.Success(service.Id);
    }
}
