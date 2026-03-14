using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Branches;
using SplashSphere.Application.Features.Branches.Commands.CreateBranch;
using SplashSphere.Application.Features.Branches.Commands.ToggleBranchStatus;
using SplashSphere.Application.Features.Branches.Commands.UpdateBranch;
using SplashSphere.Application.Features.Branches.Queries.GetBranchById;
using SplashSphere.Application.Features.Branches.Queries.GetBranches;

namespace SplashSphere.API.Endpoints;

public static class BranchEndpoints
{
    public static IEndpointRouteBuilder MapBranchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/branches")
            .WithTags("Branches")
            .RequireAuthorization();

        group.MapGet("/",      GetBranches)   .WithName("GetBranches");
        group.MapGet("/{id}",  GetBranchById) .WithName("GetBranchById");
        group.MapPost("/",     CreateBranch)  .WithName("CreateBranch");
        group.MapPut("/{id}",  UpdateBranch)  .WithName("UpdateBranch");
        group.MapPatch("/{id}/status", ToggleBranchStatus).WithName("ToggleBranchStatus");

        return app;
    }

    // ── GET /api/v1/branches?page=1&pageSize=20&search=makati ────────────────

    private static async Task<IResult> GetBranches(
        [AsParameters] GetBranchesQuery query,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result);
    }

    // ── GET /api/v1/branches/{id} ────────────────────────────────────────────

    private static async Task<IResult> GetBranchById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        // Handler throws NotFoundException → GlobalExceptionHandler → 404 ProblemDetails.
        var dto = await sender.Send(new GetBranchByIdQuery(id), ct);
        return TypedResults.Ok(dto);
    }

    // ── POST /api/v1/branches ────────────────────────────────────────────────

    private static async Task<IResult> CreateBranch(
        CreateBranchCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/branches/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/branches/{id} ────────────────────────────────────────────

    private static async Task<IResult> UpdateBranch(
        string id,
        UpdateBranchRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateBranchCommand(id, body.Name, body.Code, body.Address, body.ContactNumber),
            ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/branches/{id}/status ──────────────────────────────────

    private static async Task<IResult> ToggleBranchStatus(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ToggleBranchStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    /// <summary>PUT body — id comes from the route, not the body.</summary>
    private sealed record UpdateBranchRequest(
        string Name,
        string Code,
        string Address,
        string ContactNumber);
}
