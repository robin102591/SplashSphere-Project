using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(string Id) : ICommand;
