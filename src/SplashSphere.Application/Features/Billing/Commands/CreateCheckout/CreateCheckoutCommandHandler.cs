using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Commands.CreateCheckout;

public sealed class CreateCheckoutCommandHandler(
    IPaymentGateway paymentGateway,
    ITenantContext tenantContext)
    : IRequestHandler<CreateCheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(
        CreateCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (request.TargetPlan == PlanTier.Trial)
            return Result.Failure<CheckoutResultDto>(
                Error.Validation("Cannot checkout for the Trial plan."));

        var plan = PlanCatalog.GetPlan(request.TargetPlan);

        var session = await paymentGateway.CreateCheckoutSessionAsync(
            tenantContext.TenantId,
            request.TargetPlan,
            plan.MonthlyPrice,
            "PHP",
            request.SuccessUrl,
            request.CancelUrl,
            cancellationToken);

        return Result.Success(new CheckoutResultDto(session.CheckoutUrl, session.SessionId));
    }
}
