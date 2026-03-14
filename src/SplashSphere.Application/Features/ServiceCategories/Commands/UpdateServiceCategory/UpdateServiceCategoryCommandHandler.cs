using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;

public sealed class UpdateServiceCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateServiceCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateServiceCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("ServiceCategory", request.Id));

        var nameConflict = await context.ServiceCategories
            .AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(
                Error.Conflict($"Service category '{request.Name}' already exists."));

        category.Name        = request.Name;
        category.Description = request.Description;

        return Result.Success();
    }
}
