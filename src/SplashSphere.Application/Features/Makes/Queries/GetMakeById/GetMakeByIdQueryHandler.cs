using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Makes.Queries.GetMakeById;

public sealed class GetMakeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMakeByIdQuery, MakeDto>
{
    public async Task<MakeDto> Handle(
        GetMakeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var m = await context.Makes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Make '{request.Id}' was not found.");

        return new MakeDto(m.Id, m.Name, m.IsActive, m.CreatedAt, m.UpdatedAt);
    }
}
