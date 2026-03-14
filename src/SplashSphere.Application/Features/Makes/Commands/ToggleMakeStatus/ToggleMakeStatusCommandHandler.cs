using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Makes.Commands.ToggleMakeStatus;

public sealed class ToggleMakeStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleMakeStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleMakeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var make = await context.Makes
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (make is null)
            return Result.Failure(Error.NotFound("Make", request.Id));

        make.IsActive = !make.IsActive;

        return Result.Success();
    }
}
