using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Queue.Commands.AddToQueue;
using SplashSphere.Application.Features.Queue.Commands.CallNextInQueue;
using SplashSphere.Application.Features.Queue.Commands.CancelQueueEntry;
using SplashSphere.Application.Features.Queue.Commands.MarkNoShow;
using SplashSphere.Application.Features.Queue.Commands.RequeueEntry;
using SplashSphere.Application.Features.Queue.Commands.StartQueueService;
using SplashSphere.Application.Features.Queue.Queries.GetNextInQueue;
using SplashSphere.Application.Features.Queue.Queries.GetQueue;
using SplashSphere.Application.Features.Queue.Queries.GetQueueDisplay;
using SplashSphere.Application.Features.Queue.Queries.GetQueueEntry;
using SplashSphere.Application.Features.Queue.Queries.GetQueueStats;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class QueueEndpoints
{
    public static IEndpointRouteBuilder MapQueueEndpoints(this IEndpointRouteBuilder app)
    {
        // ── Public display endpoint — NO auth, accessible from wall TV ────────
        app.MapGet("/api/v1/queue/display", GetQueueDisplay)
            .WithTags("Queue")
            .WithName("GetQueueDisplay")
            .WithSummary("Get public queue display data")
            .AllowAnonymous();

        // ── All other queue endpoints — require auth ───────────────────────────
        var group = app
            .MapGroup("/api/v1/queue")
            .WithTags("Queue")
            .RequireAuthorization()
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.QueueManagement));

        group.MapGet("/",              GetQueue)       .WithName("GetQueue").WithSummary("Get current queue");
        group.MapGet("/next",          GetNextInQueue) .WithName("GetNextInQueue").WithSummary("Get next entry to call");
        group.MapGet("/stats",         GetQueueStats)  .WithName("GetQueueStats").WithSummary("Get queue statistics");
        group.MapGet("/{id}",          GetQueueEntry)  .WithName("GetQueueEntry").WithSummary("Get queue entry");
        group.MapPost("/",             AddToQueue)     .WithName("AddToQueue").WithSummary("Add vehicle to queue");
        group.MapPatch("/{id}/call",   CallNext)       .WithName("CallNextInQueue").WithSummary("Call next customer");
        group.MapPatch("/{id}/start",  StartService)   .WithName("StartQueueService").WithSummary("Start service from queue");
        group.MapPatch("/{id}/cancel", Cancel)         .WithName("CancelQueueEntry").WithSummary("Cancel queue entry");
        group.MapPatch("/{id}/no-show",MarkNoShow)     .WithName("MarkNoShow").WithSummary("Mark as no-show");
        group.MapPatch("/{id}/requeue",Requeue)        .WithName("RequeueEntry").WithSummary("Requeue no-show entry");

        return app;
    }

    // ── GET /api/v1/queue/display?branchId=xxx ────────────────────────────────
    // No auth — intended for wall-mounted TV. Plates are masked server-side.

    private static async Task<IResult> GetQueueDisplay(
        string branchId, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetQueueDisplayQuery(branchId), ct));

    // ── GET /api/v1/queue?branchId=&statuses=&page=&pageSize= ─────────────────
    // statuses is a comma-separated list of QueueStatus int values.

    private static async Task<IResult> GetQueue(
        string branchId, ISender sender, CancellationToken ct,
        int page = 1, int pageSize = 50, string? statuses = null)
    {
        IReadOnlyList<QueueStatus>? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(statuses))
        {
            statusFilter = statuses
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Enum.Parse<QueueStatus>(s, ignoreCase: true))
                .ToList();
        }

        return TypedResults.Ok(await sender.Send(
            new GetQueueQuery(branchId, statusFilter, page, pageSize), ct));
    }

    // ── GET /api/v1/queue/next?branchId= ─────────────────────────────────────

    private static async Task<IResult> GetNextInQueue(
        string branchId, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetNextInQueueQuery(branchId), ct));

    // ── GET /api/v1/queue/stats?branchId= ────────────────────────────────────

    private static async Task<IResult> GetQueueStats(
        string branchId, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetQueueStatsQuery(branchId), ct));

    // ── GET /api/v1/queue/{id} ────────────────────────────────────────────────

    private static async Task<IResult> GetQueueEntry(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetQueueEntryQuery(id), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }

    // ── POST /api/v1/queue ────────────────────────────────────────────────────

    private static async Task<IResult> AddToQueue(
        AddToQueueCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/queue/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PATCH /api/v1/queue/{id}/call ─────────────────────────────────────────
    // Calls the next WAITING entry in the branch. {id} is the BranchId here —
    // the endpoint picks the highest-priority entry automatically.

    private static async Task<IResult> CallNext(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CallNextInQueueCommand(id), ct);
        if (!result.IsSuccess) return result.ToProblem();

        return result.Value is null
            ? TypedResults.Ok(new { message = "No waiting entries in queue." })
            : TypedResults.Ok(new { calledQueueEntryId = result.Value });
    }

    // ── PATCH /api/v1/queue/{id}/start ────────────────────────────────────────
    // Body: { "transactionId": "..." }

    private static async Task<IResult> StartService(
        string id, StartServiceBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new StartQueueServiceCommand(id, body.TransactionId), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/queue/{id}/cancel ──────────────────────────────────────

    private static async Task<IResult> Cancel(
        string id, CancelBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelQueueEntryCommand(id, body.Reason), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/queue/{id}/no-show ─────────────────────────────────────

    private static async Task<IResult> MarkNoShow(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new MarkNoShowCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/queue/{id}/requeue ─────────────────────────────────────

    private static async Task<IResult> Requeue(
        string id, RequeueBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RequeueEntryCommand(id, body.NewPriority), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request body types ────────────────────────────────────────────────────

    private sealed record StartServiceBody(string TransactionId);
    private sealed record CancelBody(string? Reason);
    private sealed record RequeueBody(QueuePriority? NewPriority);
}
