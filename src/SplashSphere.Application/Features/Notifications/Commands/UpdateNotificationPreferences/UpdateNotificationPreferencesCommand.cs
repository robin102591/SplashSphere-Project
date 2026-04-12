using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Notifications.Commands.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommand(
    List<NotificationPreferenceItemDto> Preferences) : ICommand;
