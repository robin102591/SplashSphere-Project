using MediatR;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Customers.Queries.GetCustomerEmail;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Application.Features.Transactions.Services;
using SplashSphere.Domain.Events;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends an HTML email receipt to the customer when their transaction is
/// completed. Skips when:
/// <list type="bullet">
///   <item>The tenant's plan lacks the <c>DigitalReceipts</c> feature.</item>
///   <item>The transaction has no linked customer, or the customer has no email.</item>
/// </list>
/// Renders the receipt by calling <see cref="GetReceiptQuery"/> through the
/// existing MediatR pipeline so the email content is byte-identical to the
/// printed PDF (modulo the format).
/// </summary>
/// <remarks>
/// Email send failures are logged but do not throw — the rest of the
/// transaction-completed pipeline (loyalty points, SMS, dashboard
/// broadcast, payroll commission accumulation) must not be torn down by
/// a transient SMTP/Resend hiccup. The cashier can use the manual resend
/// endpoint (<c>POST /transactions/{id}/receipt/send</c>) to retry.
/// </remarks>
public sealed class TransactionCompletedDigitalReceiptHandler(
    ISender sender,
    IEmailService emailService,
    IPlanEnforcementService planService,
    ILogger<TransactionCompletedDigitalReceiptHandler> logger)
    : INotificationHandler<DomainEventNotification<TransactionCompletedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionCompletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // ── Plan gate ─────────────────────────────────────────────────────────
        if (!await planService.HasFeatureAsync(e.TenantId, FeatureKeys.DigitalReceipts, cancellationToken))
            return;

        // ── Build the receipt + locate the customer email ────────────────────
        // The receipt query already joins customer + tenant + branch, so we
        // reuse it here rather than projecting a separate "send-email" view.
        var receipt = await sender.Send(new GetReceiptQuery(e.TransactionId), cancellationToken);
        if (receipt is null)
        {
            logger.LogWarning("Receipt {TxId} not found while processing TransactionCompleted; skipping digital receipt.", e.TransactionId);
            return;
        }

        if (receipt.Customer is null)
            return; // walk-in with no registered customer

        // ReceiptDto doesn't carry the customer email — pull it via the
        // dedicated query for clarity. (The receipt DTO intentionally omits
        // email because it's not displayed; it's only needed for delivery.)
        var email = await sender.Send(
            new GetCustomerEmailQuery(receipt.Customer.Id),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(email))
            return; // customer is registered but has no email on file

        // ── Send ──────────────────────────────────────────────────────────────
        var html = ReceiptHtmlRenderer.Render(receipt);
        var subject = $"Receipt from {receipt.Company.BusinessName} — {receipt.TransactionNumber}";

        try
        {
            await emailService.SendAsync(
                new EmailMessage(email, subject, html),
                cancellationToken);

            logger.LogInformation(
                "Digital receipt {TxNumber} sent to {Email} for tenant {TenantId}",
                receipt.TransactionNumber, email, e.TenantId);
        }
        catch (Exception ex)
        {
            // Swallow on purpose — see the class-level <remarks/>.
            logger.LogError(ex,
                "Failed to send digital receipt {TxNumber} to {Email}",
                receipt.TransactionNumber, email);
        }
    }
}
