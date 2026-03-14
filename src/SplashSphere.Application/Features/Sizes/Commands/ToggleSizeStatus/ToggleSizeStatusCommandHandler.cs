using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Commands.ToggleSizeStatus;

public sealed class ToggleSizeStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleSizeStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleSizeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var size = await context.Sizes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (size is null)
            return Result.Failure(Error.NotFound("Size", request.Id));

        size.IsActive = !size.IsActive;

        return Result.Success();
    }
}
