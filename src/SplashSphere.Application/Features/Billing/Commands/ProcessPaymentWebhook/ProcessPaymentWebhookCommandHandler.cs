using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Commands.ProcessPaymentWebhook;

public sealed class ProcessPaymentWebhookCommandHandler(
    IApplicationDbContext db,
    IPaymentGateway paymentGateway,
    IPlanEnforcementService planService,
    ILogger<ProcessPaymentWebhookCommandHandler> logger)
    : IRequestHandler<ProcessPaymentWebhookCommand, Result>
{
    public async Task<Result> Handle(
        ProcessPaymentWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var webhookEvent = await paymentGateway.ParseWebhookAsync(
            request.Payload, request.Signature, cancellationToken);

        if (webhookEvent is null)
            return Result.Failure(Error.Validation("Invalid webhook signature or payload."));

        if (string.IsNullOrEmpty(webhookEvent.TenantId))
        {
            logger.LogWarning("Payment webhook has no tenant ID: {PaymentId}", webhookEvent.PaymentId);
            return Result.Success();
        }

        var sub = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == webhookEvent.TenantId, cancellationToken);

        if (sub is null)
        {
            logger.LogWarning("Payment webhook for unknown tenant: {TenantId}", webhookEvent.TenantId);
            return Result.Success();
        }

        if (webhookEvent.Succeeded)
        {
            var now = DateTime.UtcNow;
            var oldPlan = sub.PlanTier;

            // Update plan tier if the payment was for a plan upgrade/change
            if (webhookEvent.TargetPlan.HasValue && webhookEvent.TargetPlan.Value != sub.PlanTier)
            {
                sub.PlanTier = webhookEvent.TargetPlan.Value;

                db.PlanChangeLogs.Add(new PlanChangeLog(
                    sub.TenantId,
                    oldPlan,
                    webhookEvent.TargetPlan.Value,
                    "system",
                    $"Plan upgraded via payment ({webhookEvent.PaymentMethod})"));
            }

            sub.Status = SubscriptionStatus.Active;
            sub.LastPaymentDate = now;
            sub.CurrentPeriodStart = now;
            sub.CurrentPeriodEnd = now.AddDays(30);
            sub.NextBillingDate = now.AddDays(30);

            var billingType = oldPlan != sub.PlanTier ? BillingType.Upgrade : BillingType.Subscription;
            var billing = new BillingRecord(
                sub.TenantId,
                sub.Id,
                webhookEvent.Amount,
                billingType,
                now)
            {
                Status = BillingStatus.Paid,
                PaidDate = now,
                PaymentGatewayId = webhookEvent.PaymentId,
                PaymentMethod = webhookEvent.PaymentMethod,
                Currency = webhookEvent.Currency,
            };

            db.BillingRecords.Add(billing);

            logger.LogInformation(
                "Payment succeeded for tenant {TenantId}: ₱{Amount} via {Method}, Plan: {OldPlan} → {NewPlan}",
                sub.TenantId, webhookEvent.Amount, webhookEvent.PaymentMethod, oldPlan, sub.PlanTier);
        }
        else
        {
            if (sub.Status == SubscriptionStatus.Active)
                sub.Status = SubscriptionStatus.PastDue;

            var billing = new BillingRecord(
                sub.TenantId,
                sub.Id,
                webhookEvent.Amount,
                BillingType.Subscription,
                DateTime.UtcNow)
            {
                Status = BillingStatus.Failed,
                PaymentGatewayId = webhookEvent.PaymentId,
                PaymentMethod = webhookEvent.PaymentMethod,
                Currency = webhookEvent.Currency,
                Notes = "Payment failed",
            };

            db.BillingRecords.Add(billing);

            logger.LogWarning(
                "Payment failed for tenant {TenantId}: ₱{Amount}",
                sub.TenantId, webhookEvent.Amount);
        }

        await db.SaveChangesAsync(cancellationToken);
        planService.EvictCache(sub.TenantId);

        return Result.Success();
    }
}
