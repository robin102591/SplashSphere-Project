using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    public async Task<PagedResult<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.Notifications.AsNoTracking().AsQueryable();

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        if (request.Category.HasValue)
            query = query.Where(n => n.Category == request.Category.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto(
                n.Id,
                (int)n.Type,
                (int)n.Category,
                (int)n.Severity,
                n.Title,
                n.Message,
                n.ReferenceId,
                n.ReferenceType,
                n.ActionUrl,
                n.ActionLabel,
                n.IsRead,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<NotificationDto>.Create(items, total, request.Page, request.PageSize);
    }
}
