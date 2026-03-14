using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.ToggleMerchandiseStatus;

public sealed class ToggleMerchandiseStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleMerchandiseStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleMerchandiseStatusCommand request,
        CancellationToken cancellationToken)
    {
        var merchandise = await context.Merchandise
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (merchandise is null)
            return Result.Failure(Error.NotFound("Merchandise", request.Id));

        merchandise.IsActive = !merchandise.IsActive;

        return Result.Success();
    }
}
