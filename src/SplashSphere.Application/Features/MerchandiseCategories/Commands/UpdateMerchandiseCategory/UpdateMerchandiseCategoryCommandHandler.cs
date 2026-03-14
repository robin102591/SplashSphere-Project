using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.UpdateMerchandiseCategory;

public sealed class UpdateMerchandiseCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateMerchandiseCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateMerchandiseCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.MerchandiseCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("MerchandiseCategory", request.Id));

        var nameConflict = await context.MerchandiseCategories
            .AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(
                Error.Conflict($"Merchandise category '{request.Name}' already exists."));

        category.Name        = request.Name;
        category.Description = request.Description;

        return Result.Success();
    }
}
