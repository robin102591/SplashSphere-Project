using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Development mock that returns a fake checkout URL and auto-succeeds payments.
/// Replace with <c>PayMongoPaymentGateway</c> for production.
/// </summary>
public sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<CheckoutSession> CreateCheckoutSessionAsync(
        string tenantId,
        PlanTier targetPlan,
        decimal amount,
        string currency,
        string successUrl,
        string cancelUrl,
        CancellationToken ct,
        string? invoiceId = null)
    {
        var sessionId = $"mock_session_{Guid.NewGuid():N}";
        var checkoutUrl = $"{successUrl}?session_id={sessionId}&plan={targetPlan}&amount={amount}";

        return Task.FromResult(new CheckoutSession(sessionId, checkoutUrl));
    }

    public Task<WebhookEvent?> ParseWebhookAsync(
        string payload,
        string signature,
        CancellationToken ct)
    {
        if (signature != "mock")
            return Task.FromResult<WebhookEvent?>(null);

        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(payload);
            var root = doc.RootElement;

            PlanTier? targetPlan = null;
            if (root.TryGetProperty("planTier", out var ptVal) &&
                Enum.TryParse<PlanTier>(ptVal.GetString(), true, out var parsed))
                targetPlan = parsed;

            string? invoiceId = root.TryGetProperty("invoiceId", out var invVal) ? invVal.GetString() : null;

            return Task.FromResult<WebhookEvent?>(new WebhookEvent(
                EventType: root.GetProperty("eventType").GetString() ?? "payment.paid",
                PaymentId: root.GetProperty("paymentId").GetString() ?? $"mock_{Guid.NewGuid():N}",
                TenantId: root.TryGetProperty("tenantId", out var tid) ? tid.GetString() : null,
                TargetPlan: targetPlan,
                InvoiceId: invoiceId,
                Amount: root.TryGetProperty("amount", out var amt) ? amt.GetDecimal() : 0,
                Currency: root.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "PHP" : "PHP",
                PaymentMethod: root.TryGetProperty("paymentMethod", out var pm) ? pm.GetString() : "mock",
                Succeeded: !root.TryGetProperty("succeeded", out var s) || s.GetBoolean()));
        }
        catch
        {
            return Task.FromResult<WebhookEvent?>(null);
        }
    }
}
