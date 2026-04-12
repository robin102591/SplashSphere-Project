using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Notifications.Queries.GetNotificationPreferences;

public sealed record GetNotificationPreferencesQuery : IQuery<List<NotificationPreferenceDto>>;
