using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.TogglePackageStatus;

public sealed class TogglePackageStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<TogglePackageStatusCommand, Result>
{
    public async Task<Result> Handle(
        TogglePackageStatusCommand request,
        CancellationToken cancellationToken)
    {
        var package = await context.ServicePackages
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (package is null)
            return Result.Failure(Error.NotFound("Package", request.Id));

        package.IsActive = !package.IsActive;

        return Result.Success();
    }
}
