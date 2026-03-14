using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Models.Queries.GetModelById;

public sealed class GetModelByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetModelByIdQuery, ModelDto>
{
    public async Task<ModelDto> Handle(
        GetModelByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Project Make.Name in the same query — no separate Include needed.
        var dto = await context.Models
            .AsNoTracking()
            .Where(m => m.Id == request.Id)
            .Select(m => new ModelDto(
                m.Id, m.MakeId, m.Make.Name, m.Name, m.IsActive, m.CreatedAt, m.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Model '{request.Id}' was not found.");

        return dto;
    }
}
