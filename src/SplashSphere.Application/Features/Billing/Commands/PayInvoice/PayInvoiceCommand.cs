using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Billing.Commands.PayInvoice;

public sealed record PayInvoiceCommand(
    string BillingRecordId,
    string SuccessUrl,
    string CancelUrl) : ICommand<CheckoutResultDto>;
