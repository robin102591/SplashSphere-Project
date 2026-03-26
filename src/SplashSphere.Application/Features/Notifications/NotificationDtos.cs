namespace SplashSphere.Application.Features.Notifications;

public sealed record NotificationDto(
    string Id,
    int Type,
    int Category,
    string Title,
    string Message,
    string? ReferenceId,
    string? ReferenceType,
    bool IsRead,
    DateTime CreatedAt);

public sealed record UnreadCountDto(int Count);
