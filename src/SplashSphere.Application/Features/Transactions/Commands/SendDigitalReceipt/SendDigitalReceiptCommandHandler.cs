using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Customers.Queries.GetCustomerEmail;
using SplashSphere.Application.Features.Transactions.Queries.GetReceipt;
using SplashSphere.Application.Features.Transactions.Services;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.SendDigitalReceipt;

public sealed class SendDigitalReceiptCommandHandler(
    ISender sender,
    ITenantContext tenantContext,
    IEmailService emailService,
    IPlanEnforcementService planService)
    : IRequestHandler<SendDigitalReceiptCommand, Result>
{
    public async Task<Result> Handle(
        SendDigitalReceiptCommand request,
        CancellationToken cancellationToken)
    {
        // Plan gate — same key as the auto-send handler. Manual resend is
        // not a free workaround for non-Growth tenants.
        var hasFeature = await planService.HasFeatureAsync(
            tenantContext.TenantId,
            FeatureKeys.DigitalReceipts,
            cancellationToken);

        if (!hasFeature)
            return Result.Failure(Error.Forbidden(
                "Digital receipts require the Growth or Enterprise plan."));

        var receipt = await sender.Send(new GetReceiptQuery(request.TransactionId), cancellationToken);
        if (receipt is null)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        // Resolve email: explicit override > customer on-file email.
        string? email = request.OverrideEmail;
        if (string.IsNullOrWhiteSpace(email))
        {
            if (receipt.Customer is null)
                return Result.Failure(Error.Validation(
                    "This is a walk-in transaction with no registered customer. Provide an overrideEmail to send the receipt."));

            email = await sender.Send(
                new GetCustomerEmailQuery(receipt.Customer.Id),
                cancellationToken);

            if (string.IsNullOrWhiteSpace(email))
                return Result.Failure(Error.Validation(
                    "The customer has no email on file. Provide an overrideEmail or update the customer's profile first."));
        }

        var html = ReceiptHtmlRenderer.Render(receipt);
        var subject = $"Receipt from {receipt.Company.BusinessName} — {receipt.TransactionNumber}";

        try
        {
            await emailService.SendAsync(
                new EmailMessage(email, subject, html),
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Surface the failure to the cashier — unlike the auto-send
            // handler, the user explicitly clicked "Send", so they need to
            // know if it failed (and whether to retry / pick a different
            // delivery channel).
            return Result.Failure(Error.Validation(
                $"Email delivery failed: {ex.Message}"));
        }

        return Result.Success();
    }
}
