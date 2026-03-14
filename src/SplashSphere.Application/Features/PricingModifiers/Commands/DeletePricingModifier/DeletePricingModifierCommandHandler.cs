using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PricingModifiers.Commands.DeletePricingModifier;

public sealed class DeletePricingModifierCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeletePricingModifierCommand, Result>
{
    public async Task<Result> Handle(
        DeletePricingModifierCommand request,
        CancellationToken cancellationToken)
    {
        var modifier = await context.PricingModifiers
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (modifier is null)
            return Result.Failure(Error.NotFound("PricingModifier", request.Id));

        context.PricingModifiers.Remove(modifier);

        return Result.Success();
    }
}
