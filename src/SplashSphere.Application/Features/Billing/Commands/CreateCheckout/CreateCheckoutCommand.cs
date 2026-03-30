using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Billing.Commands.CreateCheckout;

public sealed record CreateCheckoutCommand(
    PlanTier TargetPlan,
    string SuccessUrl,
    string CancelUrl) : ICommand<CheckoutResultDto>;
