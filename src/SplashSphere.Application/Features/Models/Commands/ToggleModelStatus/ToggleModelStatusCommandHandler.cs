using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.ToggleModelStatus;

public sealed class ToggleModelStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleModelStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleModelStatusCommand request,
        CancellationToken cancellationToken)
    {
        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (model is null)
            return Result.Failure(Error.NotFound("Model", request.Id));

        model.IsActive = !model.IsActive;

        return Result.Success();
    }
}
