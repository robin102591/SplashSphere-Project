using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.TogglePricingModifierStatus;

public sealed class TogglePricingModifierStatusCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<TogglePricingModifierStatusCommand, Result>
{
    public async Task<Result> Handle(
        TogglePricingModifierStatusCommand request,
        CancellationToken cancellationToken)
    {
        var modifier = await context.PricingModifiers
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (modifier is null)
            return Result.Failure(Error.NotFound("PricingModifier", request.Id));

        modifier.IsActive = !modifier.IsActive;

        return Result.Success();
    }
}
