using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Commands.PayInvoice;

public sealed class PayInvoiceCommandHandler(
    IApplicationDbContext db,
    IPaymentGateway paymentGateway,
    ITenantContext tenantContext)
    : IRequestHandler<PayInvoiceCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(
        PayInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var record = await db.BillingRecords
            .Include(b => b.Subscription)
            .FirstOrDefaultAsync(b => b.Id == request.BillingRecordId, cancellationToken);

        if (record is null)
            return Result.Failure<CheckoutResultDto>(
                Error.NotFound("BillingRecord", request.BillingRecordId));

        if (record.Status != BillingStatus.Pending)
            return Result.Failure<CheckoutResultDto>(
                Error.Validation($"This invoice is already {record.Status}. Only pending invoices can be paid."));

        var session = await paymentGateway.CreateCheckoutSessionAsync(
            tenantContext.TenantId,
            record.Subscription.PlanTier,
            record.Amount,
            record.Currency,
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        // Store the checkout session ID on the billing record so the webhook can link them
        record.PaymentGatewayId = session.SessionId;

        return Result.Success(new CheckoutResultDto(session.CheckoutUrl, session.SessionId));
    }
}
