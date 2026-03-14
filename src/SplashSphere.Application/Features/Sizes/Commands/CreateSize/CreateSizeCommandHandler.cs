using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.CreateSize;

public sealed class CreateSizeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateSizeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateSizeCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.Sizes
            .AnyAsync(s => s.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(Error.Conflict($"Size '{request.Name}' already exists."));

        var size = new Size(tenantContext.TenantId, request.Name);
        context.Sizes.Add(size);

        return Result.Success(size.Id);
    }
}
