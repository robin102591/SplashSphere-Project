using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.ToggleServiceCategoryStatus;

public sealed class ToggleServiceCategoryStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleServiceCategoryStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleServiceCategoryStatusCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("ServiceCategory", request.Id));

        category.IsActive = !category.IsActive;

        return Result.Success();
    }
}
