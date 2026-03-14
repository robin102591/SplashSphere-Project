using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Sizes.Queries.GetSizeById;

public sealed class GetSizeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSizeByIdQuery, SizeDto>
{
    public async Task<SizeDto> Handle(
        GetSizeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var s = await context.Sizes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Size '{request.Id}' was not found.");

        return new SizeDto(s.Id, s.Name, s.IsActive, s.CreatedAt, s.UpdatedAt);
    }
}
