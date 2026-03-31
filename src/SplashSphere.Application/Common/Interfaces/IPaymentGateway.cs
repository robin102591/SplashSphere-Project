using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstraction over payment gateway (PayMongo for PH, Stripe for international).
/// Allows swapping gateway implementation without changing business logic.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Create a hosted checkout session and return its URL.</summary>
    Task<CheckoutSession> CreateCheckoutSessionAsync(
        string tenantId,
        PlanTier targetPlan,
        decimal amount,
        string currency,
        string successUrl,
        string cancelUrl,
        CancellationToken ct);

    /// <summary>Verify webhook signature and parse the payload.</summary>
    Task<WebhookEvent?> ParseWebhookAsync(string payload, string signature, CancellationToken ct);
}

public sealed record CheckoutSession(string SessionId, string CheckoutUrl);

public sealed record WebhookEvent(
    string EventType,
    string PaymentId,
    string? TenantId,
    PlanTier? TargetPlan,
    decimal Amount,
    string Currency,
    string? PaymentMethod,
    bool Succeeded);
