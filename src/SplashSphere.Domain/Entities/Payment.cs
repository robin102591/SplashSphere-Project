namespace SplashSphere.Domain.Entities;

/// <summary>
/// A single payment instalment applied to a <see cref="Transaction"/>.
/// A transaction may have multiple payment records (split payment across methods),
/// but their <see cref="Amount"/> values must sum to <c>Transaction.FinalAmount</c>
/// before the transaction can be completed.
/// <para>
/// <see cref="ReferenceNumber"/> should be recorded for non-cash payments
/// (GCash reference, card approval code, bank transfer ref) for reconciliation.
/// </para>
/// Cascade delete: deleted when parent <see cref="Transaction"/> is deleted.
/// </summary>
public sealed class Payment : IAuditableEntity, ITenantScoped
{
    private Payment() { } // EF Core

    public Payment(
        string tenantId,
        string transactionId,
        PaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionId = transactionId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        ReferenceNumber = referenceNumber;
        PaidAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Amount covered by this payment instalment. Precision (10, 2).</summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// GCash reference number, card approval code, or bank transfer reference.
    /// Required for non-cash methods; null for <see cref="PaymentMethod.Cash"/>.
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>UTC timestamp when this payment was recorded.</summary>
    public DateTime PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Transaction Transaction { get; set; } = null!;
}
