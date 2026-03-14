using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.UpdateModel;

public sealed class UpdateModelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateModelCommand, Result>
{
    public async Task<Result> Handle(
        UpdateModelCommand request,
        CancellationToken cancellationToken)
    {
        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (model is null)
            return Result.Failure(Error.NotFound("Model", request.Id));

        // Name must be unique within the same make.
        var nameConflict = await context.Models
            .AnyAsync(m => m.MakeId == model.MakeId && m.Name == request.Name && m.Id != request.Id,
                cancellationToken);

        if (nameConflict)
            return Result.Failure(
                Error.Conflict($"Model '{request.Name}' already exists for this make."));

        model.Name = request.Name;

        return Result.Success();
    }
}
