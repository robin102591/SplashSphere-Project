using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Billing.Commands.ProcessPaymentWebhook;

public sealed record ProcessPaymentWebhookCommand(
    string Payload,
    string Signature) : ICommand;
