using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.ToggleMerchandiseCategoryStatus;

public sealed class ToggleMerchandiseCategoryStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleMerchandiseCategoryStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleMerchandiseCategoryStatusCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.MerchandiseCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("MerchandiseCategory", request.Id));

        category.IsActive = !category.IsActive;

        return Result.Success();
    }
}
