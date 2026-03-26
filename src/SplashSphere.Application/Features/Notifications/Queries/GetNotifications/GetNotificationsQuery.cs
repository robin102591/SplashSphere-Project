using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false) : IQuery<PagedResult<NotificationDto>>;
