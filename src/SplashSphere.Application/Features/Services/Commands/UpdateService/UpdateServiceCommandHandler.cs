using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Commands.UpdateService;

public sealed class UpdateServiceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateServiceCommand, Result>
{
    public async Task<Result> Handle(
        UpdateServiceCommand request,
        CancellationToken cancellationToken)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
            return Result.Failure(Error.NotFound("Service", request.Id));

        var categoryExists = await context.ServiceCategories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return Result.Failure(Error.NotFound("ServiceCategory", request.CategoryId));

        var nameConflict = await context.Services
            .AnyAsync(s => s.Name == request.Name && s.Id != request.Id, cancellationToken);

        if (nameConflict)
            return Result.Failure(Error.Conflict($"Service '{request.Name}' already exists."));

        service.CategoryId   = request.CategoryId;
        service.Name         = request.Name;
        service.BasePrice    = request.BasePrice;
        service.Description  = request.Description;

        return Result.Success();
    }
}
