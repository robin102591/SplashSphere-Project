using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.ToggleServiceStatus;

public sealed class ToggleServiceStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleServiceStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleServiceStatusCommand request,
        CancellationToken cancellationToken)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
            return Result.Failure(Error.NotFound("Service", request.Id));

        service.IsActive = !service.IsActive;

        return Result.Success();
    }
}
