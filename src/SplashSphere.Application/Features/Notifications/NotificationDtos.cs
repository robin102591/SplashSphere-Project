namespace SplashSphere.Application.Features.Notifications;

public sealed record NotificationDto(
    string Id,
    int Type,
    int Category,
    int Severity,
    string Title,
    string Message,
    string? ReferenceId,
    string? ReferenceType,
    string? ActionUrl,
    string? ActionLabel,
    bool IsRead,
    DateTime CreatedAt);

public sealed record UnreadCountDto(int Count);

public sealed record NotificationPreferenceDto(
    int NotificationType,
    string TypeName,
    int Category,
    bool SmsAvailable,
    bool SmsMandatory,
    bool EmailAvailable,
    bool EmailMandatory,
    bool SmsEnabled,
    bool EmailEnabled);

public sealed record UpdateNotificationPreferencesRequest(
    List<NotificationPreferenceItemDto> Preferences);

public sealed record NotificationPreferenceItemDto(
    int NotificationType,
    bool SmsEnabled,
    bool EmailEnabled);
