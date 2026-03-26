using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetUnreadCountQuery, UnreadCountDto>
{
    public async Task<UnreadCountDto> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await db.Notifications
            .AsNoTracking()
            .CountAsync(n => !n.IsRead, cancellationToken);

        return new UnreadCountDto(count);
    }
}
