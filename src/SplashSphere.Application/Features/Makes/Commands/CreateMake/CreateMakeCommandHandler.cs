using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.CreateMake;

public sealed class CreateMakeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateMakeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateMakeCommand request,
        CancellationToken cancellationToken)
    {
        var nameExists = await context.Makes
            .AnyAsync(m => m.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(Error.Conflict($"Make '{request.Name}' already exists."));

        var make = new Make(tenantContext.TenantId, request.Name);
        context.Makes.Add(make);

        return Result.Success(make.Id);
    }
}
