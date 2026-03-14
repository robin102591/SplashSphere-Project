using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.CreateModel;

public sealed class CreateModelCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateModelCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateModelCommand request,
        CancellationToken cancellationToken)
    {
        var makeExists = await context.Makes
            .AnyAsync(m => m.Id == request.MakeId, cancellationToken);

        if (!makeExists)
            return Result.Failure<string>(Error.NotFound("Make", request.MakeId));

        var nameExists = await context.Models
            .AnyAsync(m => m.MakeId == request.MakeId && m.Name == request.Name, cancellationToken);

        if (nameExists)
            return Result.Failure<string>(
                Error.Conflict($"Model '{request.Name}' already exists for this make."));

        var model = new Model(tenantContext.TenantId, request.MakeId, request.Name);
        context.Models.Add(model);

        return Result.Success(model.Id);
    }
}
