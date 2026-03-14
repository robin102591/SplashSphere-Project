using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.UpdateMake;

public sealed class UpdateMakeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateMakeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateMakeCommand request,
        CancellationToken cancellationToken)
    {
        var make = await context.Makes
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (make is null)
            return Result.Failure(Error.NotFound("Make", request.Id));

        var nameConflict = await context.Makes
            .AnyAsync(m => m.Name == request.Name && m.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(Error.Conflict($"Make '{request.Name}' already exists."));

        make.Name = request.Name;

        return Result.Success();
    }
}
