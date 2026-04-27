using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// On transaction completion, check whether the just-completed customer is the
/// referred party of a still-Pending referral at this tenant. If so, award both
/// participants their point rewards, mark the referral Completed, and publish
/// <see cref="ReferralCompletedEvent"/> so downstream notification handlers can fire.
/// <para>
/// Guards: only the customer's first completed transaction triggers the payout.
/// If either participant has no active <see cref="MembershipCard"/>, that side is
/// skipped but the referral still completes (so pending codes don't linger forever).
/// </para>
/// </summary>
public sealed class TransactionCompletedReferralHandler(
    IApplicationDbContext db,
    IPlanEnforcementService planService,
    IEventPublisher eventPublisher,
    ILogger<TransactionCompletedReferralHandler> logger)
    : INotificationHandler<DomainEventNotification<TransactionCompletedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionCompletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Feature gate — same gate used by GetReferralCode / ApplyReferral.
        if (!await planService.HasFeatureAsync(e.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken))
            return;

        // Load the transaction's customer — required to match against the referral.
        var tx = await db.Transactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.Id == e.TransactionId)
            .Select(t => new { t.CustomerId })
            .FirstOrDefaultAsync(cancellationToken);

        if (tx is null || tx.CustomerId is null)
            return;

        // Find a Pending referral where this customer is the referred party.
        var referral = await db.Referrals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == e.TenantId
                  && r.ReferredCustomerId == tx.CustomerId
                  && r.Status == ReferralStatus.Pending,
                cancellationToken);

        if (referral is null)
            return;

        // First-wash gate — only complete the referral on the customer's initial
        // completed transaction at this tenant. Count Completed including this tx.
        var completedCount = await db.Transactions
            .IgnoreQueryFilters()
            .CountAsync(
                t => t.TenantId == e.TenantId
                  && t.CustomerId == tx.CustomerId
                  && t.Status == TransactionStatus.Completed,
                cancellationToken);

        if (completedCount > 1)
        {
            // Not their first wash — leave the referral alone.
            return;
        }

        var now = DateTime.UtcNow;

        // Award the referrer (if they have an active card).
        var referrerCard = await db.MembershipCards
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                m => m.TenantId == e.TenantId
                  && m.CustomerId == referral.ReferrerCustomerId
                  && m.IsActive,
                cancellationToken);

        if (referrerCard is not null && referral.ReferrerPointsEarned > 0)
        {
            referrerCard.PointsBalance       += referral.ReferrerPointsEarned;
            referrerCard.LifetimePointsEarned += referral.ReferrerPointsEarned;

            db.PointTransactions.Add(new PointTransaction(
                e.TenantId,
                referrerCard.Id,
                PointTransactionType.Referral,
                referral.ReferrerPointsEarned,
                referrerCard.PointsBalance,
                $"Referral bonus — code {referral.ReferralCode}")
            {
                TransactionId = e.TransactionId,
            });
        }

        // Award the referred customer (if they have an active card).
        var referredCard = await db.MembershipCards
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                m => m.TenantId == e.TenantId
                  && m.CustomerId == tx.CustomerId
                  && m.IsActive,
                cancellationToken);

        if (referredCard is not null && referral.ReferredPointsEarned > 0)
        {
            referredCard.PointsBalance       += referral.ReferredPointsEarned;
            referredCard.LifetimePointsEarned += referral.ReferredPointsEarned;

            db.PointTransactions.Add(new PointTransaction(
                e.TenantId,
                referredCard.Id,
                PointTransactionType.Referral,
                referral.ReferredPointsEarned,
                referredCard.PointsBalance,
                $"Welcome bonus — referred by code {referral.ReferralCode}")
            {
                TransactionId = e.TransactionId,
            });
        }

        // Flip the referral and publish the completion event.
        referral.Status      = ReferralStatus.Completed;
        referral.CompletedAt = now;

        // No SaveChangesAsync here — EventPublisher.FlushAsync persists after all handlers.

        eventPublisher.Enqueue(new ReferralCompletedEvent(
            referral.Id,
            e.TenantId,
            e.BranchId,
            referral.ReferrerCustomerId,
            tx.CustomerId!,
            referral.ReferrerPointsEarned,
            referral.ReferredPointsEarned,
            referral.ReferralCode,
            e.TransactionId));

        logger.LogInformation(
            "Referral {ReferralId} completed — referrer +{ReferrerPts}, referred +{ReferredPts}.",
            referral.Id, referral.ReferrerPointsEarned, referral.ReferredPointsEarned);
    }
}
