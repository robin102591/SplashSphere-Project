using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records each billing event — subscription payment, upgrade, refund, etc.
/// Separate from the payment gateway so the system is gateway-agnostic.
/// </summary>
public sealed class BillingRecord : IAuditableEntity
{
    private BillingRecord() { } // EF Core

    public BillingRecord(
        string tenantId,
        string subscriptionId,
        decimal amount,
        BillingType type,
        DateTime billingDate)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        Amount = amount;
        Type = type;
        BillingDate = billingDate;
        Status = BillingStatus.Pending;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PHP";
    public BillingType Type { get; set; }
    public BillingStatus Status { get; set; }
    public string? PaymentGatewayId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime BillingDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
    public TenantSubscription Subscription { get; set; } = null!;
}
