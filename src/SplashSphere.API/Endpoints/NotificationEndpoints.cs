using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Notifications;
using SplashSphere.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using SplashSphere.Application.Features.Notifications.Commands.MarkNotificationRead;
using SplashSphere.Application.Features.Notifications.Commands.UpdateNotificationPreferences;
using SplashSphere.Application.Features.Notifications.Queries.GetNotifications;
using SplashSphere.Application.Features.Notifications.Queries.GetNotificationPreferences;
using SplashSphere.Application.Features.Notifications.Queries.GetUnreadCount;

namespace SplashSphere.API.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/",                GetNotifications)      .WithName("GetNotifications");
        group.MapGet("/unread-count",    GetUnreadCount)        .WithName("GetUnreadCount");
        group.MapPatch("/{id}/read",     MarkNotificationRead)  .WithName("MarkNotificationRead");
        group.MapPost("/mark-all-read",  MarkAllRead)           .WithName("MarkAllNotificationsRead");
        group.MapGet("/preferences",     GetPreferences)        .WithName("GetNotificationPreferences");
        group.MapPut("/preferences",     UpdatePreferences)     .WithName("UpdateNotificationPreferences");

        return app;
    }

    private static async Task<IResult> GetNotifications(
        [AsParameters] GetNotificationsQuery query,
        ISender sender,
        CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetUnreadCount(
        ISender sender,
        CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetUnreadCountQuery(), ct));

    private static async Task<IResult> MarkNotificationRead(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new MarkNotificationReadCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> MarkAllRead(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new MarkAllNotificationsReadCommand(), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> GetPreferences(
        ISender sender,
        CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetNotificationPreferencesQuery(), ct));

    private static async Task<IResult> UpdatePreferences(
        UpdateNotificationPreferencesRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateNotificationPreferencesCommand(request.Preferences), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }
}
