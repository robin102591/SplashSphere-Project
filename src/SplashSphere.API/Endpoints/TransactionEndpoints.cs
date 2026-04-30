using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Transactions.Commands.AddPayment;
using SplashSphere.Application.Features.Transactions.Commands.CreateTransaction;
using SplashSphere.Application.Features.Transactions.Commands.RefundTransaction;
using SplashSphere.Application.Features.Transactions.Commands.UpdateDiscountTip;
using SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionItems;
using SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionStatus;
using SplashSphere.Application.Features.Transactions.Queries.GetDailySummary;
using SplashSphere.Application.Features.Transactions.Commands.SendDigitalReceipt;
using SplashSphere.Application.Features.Transactions.Queries.ExportReceiptPdf;
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
        group.MapPost("/",                           CreateTransaction).WithSummary("Create transaction");
        group.MapPatch("/{id}/status",               UpdateTransactionStatus).WithSummary("Update transaction status");
        group.MapPatch("/{id}/items",                UpdateTransactionItems).WithSummary("Update transaction line items");
        group.MapPatch("/{id}/discount-tip",         UpdateDiscountTip).WithSummary("Update discount and tip");
        group.MapPost("/{id}/payments",              AddPayment).WithSummary("Add payment to transaction");
        group.MapPost("/{id}/refund",                RefundTransaction).WithSummary("Refund transaction");

        // ── Queries ───────────────────────────────────────────────────────────
        // daily-summary must come BEFORE /{id} to avoid route ambiguity
        group.MapGet("/daily-summary",               GetDailySummary).WithSummary("Get daily summary");
        group.MapGet("/",                            GetTransactions).WithSummary("List transactions");
        group.MapGet("/{id}",                        GetTransactionById).WithSummary("Get transaction by ID");
        group.MapGet("/{id}/receipt",                GetReceipt).WithSummary("Get receipt data");
        group.MapGet("/{id}/receipt/pdf",            ExportReceiptPdf).WithSummary("Download receipt PDF");
        group.MapPost("/{id}/receipt/send",          SendReceipt).WithSummary("Email the digital receipt to the customer (or to an override address). Gated on the digital_receipts feature.");

        return app;
    }

    // ── POST / ────────────────────────────────────────────────────────────────

    private static async Task<IResult> CreateTransaction(
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
            body.QueueEntryId,
            body.PosStationId);

        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/transactions/{result.Value}", new CreateTransactionResponse(result.Value!))
            : result.ToProblem();
    }

    // ── PATCH /{id}/status ────────────────────────────────────────────────────

    private static async Task<IResult> UpdateTransactionStatus(
        string id,
        [FromBody] UpdateStatusRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateTransactionStatusCommand(id, body.NewStatus), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /{id}/items ─────────────────────────────────────────────────────

    private static async Task<IResult> UpdateTransactionItems(
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
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /{id}/payments ────────────────────────────────────────────────────

    private static async Task<IResult> AddPayment(
        string id,
        [FromBody] AddPaymentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AddPaymentCommand(id, body.PaymentMethod, body.Amount, body.ReferenceNumber), ct);

        return result.IsSuccess
            ? TypedResults.Created(
                $"/api/v1/transactions/{id}/payments/{result.Value}",
                new AddPaymentResponse(result.Value!))
            : result.ToProblem();
    }

    // ── PATCH /{id}/discount-tip ──────────────────────────────────────────────

    private static async Task<IResult> UpdateDiscountTip(
        string id,
        [FromBody] UpdateDiscountTipRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateDiscountTipCommand(id, body.DiscountAmount, body.TipAmount), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /{id}/refund ─────────────────────────────────────────────────────

    private static async Task<IResult> RefundTransaction(
        string id,
        [FromBody] RefundRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new RefundTransactionCommand(id, body.Reason), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── GET / ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> GetTransactions(
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
        return TypedResults.Ok(result);
    }

    // ── GET /{id} ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetTransactionById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTransactionByIdQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    // ── GET /{id}/receipt ─────────────────────────────────────────────────────

    private static async Task<IResult> GetReceipt(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetReceiptQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }

    // ── GET /{id}/receipt/pdf ──────────────────────────────────────────────

    private static async Task<IResult> ExportReceiptPdf(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ExportReceiptPdfQuery(id), ct);

        if (result is null)
            return TypedResults.NotFound();

        return TypedResults.File(result.Content, "application/pdf", result.FileName);
    }

    // ── POST /{id}/receipt/send ────────────────────────────────────────────

    private static async Task<IResult> SendReceipt(
        string id,
        [FromBody] SendReceiptRequest? body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new SendDigitalReceiptCommand(id, body?.OverrideEmail), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    /// <summary>
    /// Optional body for <c>POST /{id}/receipt/send</c>. When omitted entirely,
    /// the email goes to the customer's on-file address; when supplied with a
    /// value, the email goes there instead — useful when the cashier needs to
    /// resend to a corrected address without touching the customer profile.
    /// </summary>
    private sealed record SendReceiptRequest(string? OverrideEmail);

    // ── GET /daily-summary ────────────────────────────────────────────────────

    private static async Task<IResult> GetDailySummary(
        [AsParameters] DailySummaryParams p,
        ISender sender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(p.BranchId))
            return TypedResults.BadRequest(new ProblemDetails { Detail = "branchId is required." });

        var result = await sender.Send(new GetDailySummaryQuery(p.BranchId!, p.Date), ct);
        return TypedResults.Ok(result);
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
        string? QueueEntryId,
        string? PosStationId);

    private sealed record CreateTransactionResponse(string TransactionId);

    private sealed record UpdateStatusRequest(TransactionStatus NewStatus);

    private sealed record UpdateTransactionItemsRequest(
        IReadOnlyList<TransactionServiceRequest> Services,
        IReadOnlyList<TransactionPackageRequest> Packages,
        IReadOnlyList<TransactionMerchandiseRequest> Merchandise,
        decimal DiscountAmount,
        string? Notes);

    private sealed record UpdateDiscountTipRequest(decimal DiscountAmount, decimal TipAmount);

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
