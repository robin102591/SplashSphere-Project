using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 20,
    bool UnreadOnly = false,
    NotificationCategory? Category = null) : IQuery<PagedResult<NotificationDto>>;
