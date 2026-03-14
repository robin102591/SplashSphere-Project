using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.UpdateSize;

public sealed class UpdateSizeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSizeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSizeCommand request,
        CancellationToken cancellationToken)
    {
        var size = await context.Sizes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (size is null)
            return Result.Failure(Error.NotFound("Size", request.Id));

        var nameConflict = await context.Sizes
            .AnyAsync(s => s.Name == request.Name && s.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(Error.Conflict($"Size '{request.Name}' already exists."));

        size.Name = request.Name;

        return Result.Success();
    }
}
