using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Billing.Commands.CancelSubscription;
using SplashSphere.Application.Features.Billing.Commands.ChangePlan;
using SplashSphere.Application.Features.Billing.Commands.CreateCheckout;
using SplashSphere.Application.Features.Billing.Commands.PayInvoice;
using SplashSphere.Application.Features.Billing.Commands.ProcessPaymentWebhook;
using SplashSphere.Application.Features.Billing.Queries.ExportInvoicePdf;
using SplashSphere.Application.Features.Billing.Queries.GetBillingHistory;
using SplashSphere.Application.Features.Billing.Queries.GetCurrentPlan;
using SplashSphere.Domain.Enums;

namespace SplashSphere.API.Endpoints;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/billing")
            .RequireAuthorization()
            .WithTags("Billing");

        group.MapGet("/plan", GetCurrentPlan).WithSummary("Get current plan and usage");
        group.MapPost("/checkout", CreateCheckout).WithSummary("Create checkout session for plan upgrade");
        group.MapPost("/change-plan", ChangePlan).WithSummary("Change subscription plan");
        group.MapPost("/cancel", CancelSubscription).WithSummary("Cancel subscription");
        group.MapGet("/history", GetBillingHistory).WithSummary("Get billing and payment history");
        group.MapGet("/invoices/{id}/pdf", ExportInvoicePdf).WithSummary("Download invoice as PDF");
        group.MapPost("/invoices/{id}/pay", PayInvoice).WithSummary("Pay a pending invoice");

        // ── Payment webhook — NO auth (called by PayMongo) ───────────────────
        app.MapPost("/api/v1/webhooks/payment", ProcessPaymentWebhook)
            .WithTags("Webhooks")
            .AllowAnonymous()
            .WithSummary("Process payment gateway webhook");

        return app;
    }

    private static async Task<Ok<object>> GetCurrentPlan(
        ISender sender, CancellationToken ct)
        => TypedResults.Ok<object>(
            await sender.Send(new GetCurrentPlanQuery(), ct));

    private static async Task<IResult> CreateCheckout(
        [FromBody] CreateCheckoutRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateCheckoutCommand((PlanTier)body.TargetPlan, body.SuccessUrl, body.CancelUrl), ct);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> ChangePlan(
        [FromBody] ChangePlanRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ChangePlanCommand((PlanTier)body.NewPlan), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> CancelSubscription(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelSubscriptionCommand(), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<Ok<object>> GetBillingHistory(
        [AsParameters] BillingHistoryParams p,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok<object>(
            await sender.Send(new GetBillingHistoryQuery(p.Page, p.PageSize), ct));

    private static async Task<IResult> ProcessPaymentWebhook(
        HttpRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var payload = await new StreamReader(request.Body).ReadToEndAsync(ct);
        var signature = request.Headers["Paymongo-Signature"].FirstOrDefault() ?? "";

        var result = await sender.Send(
            new ProcessPaymentWebhookCommand(payload, signature), ct);

        // Always return 200 to the gateway so it doesn't retry
        return TypedResults.Ok(new { received = true });
    }

    // ── GET /invoices/{id}/pdf ───────────────────────────────────────────

    private static async Task<IResult> ExportInvoicePdf(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ExportInvoicePdfQuery(id), ct);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.File(result.Content, "application/pdf", result.FileName);
    }

    // ── POST /invoices/{id}/pay ──────────────────────────────────────────

    private static async Task<IResult> PayInvoice(
        string id,
        [FromBody] PayInvoiceRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new PayInvoiceCommand(id, body.SuccessUrl, body.CancelUrl), ct);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }

    // ── Request records ─────────────────────────────────────────────────────

    private sealed record CreateCheckoutRequest(
        int TargetPlan,
        string SuccessUrl,
        string CancelUrl);

    private sealed record ChangePlanRequest(int NewPlan);

    private sealed record PayInvoiceRequest(
        string SuccessUrl,
        string CancelUrl);

    private sealed record BillingHistoryParams(
        int Page = 1,
        int PageSize = 20);
}
