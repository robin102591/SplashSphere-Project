using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery : IQuery<UnreadCountDto>;
