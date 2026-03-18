using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Transactions.Commands.AddPayment;
using SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;
using SplashSphere.Application.Features.Transactions.Commands.RefundTransaction;
using SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionItems;
using SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionStatus;
using SplashSphere.Application.Features.Transactions.Queries.GetDailySummary;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Application.Features.Transactions.Queries.GetTransactionById;
using SplashSphere.Application.Features.Transactions.Queries.GetTransactions;
using SplashSphere.Domain.Enums;

namespace SplashSphere.API.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/transactions")
            .RequireAuthorization()
            .WithTags("Transactions");

        // ── Commands ──────────────────────────────────────────────────────────
        group.MapPost("/",                           CreateTransaction);
        group.MapPatch("/{id}/status",               UpdateTransactionStatus);
        group.MapPatch("/{id}/items",                UpdateTransactionItems);
        group.MapPost("/{id}/payments",              AddPayment);
        group.MapPost("/{id}/refund",                RefundTransaction);

        // ── Queries ───────────────────────────────────────────────────────────
        // daily-summary must come BEFORE /{id} to avoid route ambiguity
        group.MapGet("/daily-summary",               GetDailySummary);
        group.MapGet("/",                            GetTransactions);
        group.MapGet("/{id}",                        GetTransactionById);
        group.MapGet("/{id}/receipt",                GetReceipt);

        return app;
    }

    // ── POST / ────────────────────────────────────────────────────────────────

    private static async Task<Results<Created<CreateTransactionResponse>, BadRequest<ProblemDetails>>> CreateTransaction(
        [FromBody] CreateTransactionRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateTransactionCommand(
            body.BranchId,
            body.CarId,
            body.PlateNumber,
            body.VehicleTypeId,
            body.SizeId,
            body.CustomerId,
            body.Services,
            body.Packages,
            body.Merchandise,
            body.DiscountAmount,
            body.TaxAmount,
            body.TipAmount,
            body.Notes,
            body.QueueEntryId);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/transactions/{result.Value}", new CreateTransactionResponse(result.Value!));
    }

    // ── PATCH /{id}/status ────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdateTransactionStatus(
        string id,
        [FromBody] UpdateStatusRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateTransactionStatusCommand(id, body.NewStatus), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/items ─────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdateTransactionItems(
        string id,
        [FromBody] UpdateTransactionItemsRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateTransactionItemsCommand(
            id,
            body.Services,
            body.Packages,
            body.Merchandise,
            body.DiscountAmount,
            body.Notes);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── POST /{id}/payments ────────────────────────────────────────────────────

    private static async Task<Results<Created<AddPaymentResponse>, NotFound, BadRequest<ProblemDetails>>> AddPayment(
        string id,
        [FromBody] AddPaymentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AddPaymentCommand(id, body.PaymentMethod, body.Amount, body.ReferenceNumber), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.Created(
            $"/api/v1/transactions/{id}/payments/{result.Value}",
            new AddPaymentResponse(result.Value!));
    }

    // ── POST /{id}/refund ─────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> RefundTransaction(
        string id,
        [FromBody] RefundRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RefundTransactionCommand(id, body.Reason), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── GET / ─────────────────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetTransactions(
        [AsParameters] GetTransactionsParams p,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTransactionsQuery(
            p.BranchId!,
            p.Page,
            p.PageSize,
            p.Status,
            p.DateFrom,
            p.DateTo,
            p.Search);

        var result = await sender.Send(query, ct);
        return TypedResults.Ok<object>(result);
    }

    // ── GET /{id} ─────────────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetTransactionById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok<object>(result);
    }

    // ── GET /{id}/receipt ─────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetReceipt(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetReceiptQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok<object>(result);
    }

    // ── GET /daily-summary ────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, BadRequest<ProblemDetails>>> GetDailySummary(
        [AsParameters] DailySummaryParams p,
        ISender sender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(p.BranchId))
            return TypedResults.BadRequest(new ProblemDetails { Detail = "branchId is required." });

        var result = await sender.Send(new GetDailySummaryQuery(p.BranchId!, p.Date), ct);
        return TypedResults.Ok<object>(result);
    }

    // ── Request / response records ─────────────────────────────────────────────

    private sealed record CreateTransactionRequest(
        string BranchId,
        string? CarId,
        string? PlateNumber,
        string? VehicleTypeId,
        string? SizeId,
        string? CustomerId,
        IReadOnlyList<TransactionServiceRequest> Services,
        IReadOnlyList<TransactionPackageRequest> Packages,
        IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
        decimal DiscountAmount,
        decimal TaxAmount,
        decimal TipAmount,
        string? Notes,
        string? QueueEntryId);

    private sealed record CreateTransactionResponse(string TransactionId);

    private sealed record UpdateStatusRequest(TransactionStatus NewStatus);

    private sealed record UpdateTransactionItemsRequest(
        IReadOnlyList<TransactionServiceRequest> Services,
        IReadOnlyList<TransactionPackageRequest> Packages,
        IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
        decimal DiscountAmount,
        string? Notes);

    private sealed record AddPaymentRequest(
        PaymentMethod PaymentMethod,
        decimal Amount,
        string? ReferenceNumber);

    private sealed record AddPaymentResponse(string PaymentId);

    private sealed record RefundRequest(string? Reason);

    // ── Query parameter records ────────────────────────────────────────────────

    private sealed record GetTransactionsParams(
        string? BranchId,
        int Page = 1,
        int PageSize = 20,
        TransactionStatus? Status = null,
        DateOnly? DateFrom = null,
        DateOnly? DateTo = null,
        string? Search = null);

    private sealed record DailySummaryParams(
        string? BranchId,
        DateOnly? Date = null);
}
