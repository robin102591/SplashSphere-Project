using MediatR;
using SplashSphere.Application.Features.AuditLogs.Queries.GetAuditLogs;

namespace SplashSphere.API.Endpoints;

public static class AuditLogEndpoints
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/audit-logs")
            .RequireAuthorization()
            .WithTags("AuditLogs");

        group.MapGet("/", GetAuditLogs).WithName("GetAuditLogs").WithSummary("List audit logs");

        return app;
    }

    private static async Task<IResult> GetAuditLogs(
        [AsParameters] AuditLogParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetAuditLogsQuery(p.EntityType, p.EntityId, p.UserId, p.From, p.To, p.Page, p.PageSize), ct);
        return TypedResults.Ok(result);
    }

    private sealed record AuditLogParams(
        string? EntityType = null,
        string? EntityId = null,
        string? UserId = null,
        DateTime? From = null,
        DateTime? To = null,
        int Page = 1,
        int PageSize = 50);
}
