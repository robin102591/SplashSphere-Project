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
/// Auto-awards loyalty points when a transaction completes.
/// Checks: loyalty feature enabled, program active, customer has membership card.
/// Calculates points using earning rate × tier multiplier, updates balance,
/// and checks for tier upgrades.
/// </summary>
public sealed class TransactionCompletedLoyaltyHandler(
    IApplicationDbContext db,
    IPlanEnforcementService planService,
    IEventPublisher eventPublisher,
    ILogger<TransactionCompletedLoyaltyHandler> logger)
    : INotificationHandler<DomainEventNotification<TransactionCompletedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionCompletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Gate: tenant must have loyalty feature
        if (!await planService.HasFeatureAsync(e.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken))
            return;

        // Load transaction to get CustomerId
        var tx = await db.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.Id == e.TransactionId)
            .Select(t => new { t.CustomerId, t.FinalAmount, t.BranchId })
            .FirstOrDefaultAsync(cancellationToken);

        if (tx is null || tx.CustomerId is null)
            return;

        // Load loyalty settings
        var settings = await db.LoyaltyProgramSettings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.TenantId == e.TenantId && s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            return;

        // Get or auto-enroll membership card
        var card = await db.MembershipCards
            .IgnoreQueryFilters()
            .Where(m => m.TenantId == e.TenantId && m.CustomerId == tx.CustomerId && m.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (card is null)
        {
            if (!settings.AutoEnroll)
                return;

            // Auto-enroll
            var maxCard = await db.MembershipCards
                .IgnoreQueryFilters()
                .Where(m => m.TenantId == e.TenantId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => m.CardNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var nextSeq = 1;
            if (maxCard is not null && maxCard.StartsWith("SS-") && int.TryParse(maxCard[3..], out var current))
                nextSeq = current + 1;

            card = new MembershipCard(e.TenantId, tx.CustomerId, $"SS-{nextSeq:D5}");
            db.MembershipCards.Add(card);

            logger.LogInformation(
                "Auto-enrolled customer {CustomerId} in loyalty program with card {CardNumber}.",
                tx.CustomerId, card.CardNumber);
        }

        // Calculate base points: floor(FinalAmount / CurrencyUnitAmount) × PointsPerCurrencyUnit
        if (settings.CurrencyUnitAmount <= 0)
            return;

        var basePoints = (int)Math.Floor(tx.FinalAmount / settings.CurrencyUnitAmount) *
                         (int)settings.PointsPerCurrencyUnit;

        if (basePoints <= 0)
            return;

        // Apply tier multiplier
        var tiers = await db.LoyaltyTierConfigs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.TenantId == e.TenantId)
            .OrderByDescending(t => t.MinimumLifetimePoints)
            .ToListAsync(cancellationToken);

        var currentTierConfig = tiers
            .FirstOrDefault(t => card.LifetimePointsEarned >= t.MinimumLifetimePoints);

        var multiplier = currentTierConfig?.PointsMultiplier ?? 1m;
        var finalPoints = (int)Math.Floor(basePoints * multiplier);

        if (finalPoints <= 0)
            return;

        // Update card
        card.PointsBalance += finalPoints;
        card.LifetimePointsEarned += finalPoints;

        // Create ledger entry
        var pointTx = new PointTransaction(
            e.TenantId,
            card.Id,
            PointTransactionType.Earned,
            finalPoints,
            card.PointsBalance,
            $"Earned from {e.TransactionNumber}")
        {
            TransactionId = e.TransactionId,
            ExpiresAt = settings.PointsExpirationMonths.HasValue
                ? DateTime.UtcNow.AddMonths(settings.PointsExpirationMonths.Value)
                : null,
        };

        db.PointTransactions.Add(pointTx);

        // Update transaction's PointsEarned for display
        var transaction = await db.Transactions
            .IgnoreQueryFilters()
            .FirstAsync(t => t.Id == e.TransactionId, cancellationToken);

        transaction.PointsEarned = finalPoints;

        // Check tier upgrade
        var previousTier = card.CurrentTier;
        var newTierConfig = tiers
            .Where(t => card.LifetimePointsEarned >= t.MinimumLifetimePoints)
            .OrderByDescending(t => t.Tier)
            .FirstOrDefault();

        if (newTierConfig is not null && newTierConfig.Tier > card.CurrentTier)
        {
            card.CurrentTier = newTierConfig.Tier;

            eventPublisher.Enqueue(new TierUpgradedEvent(
                card.Id, e.TenantId, tx.CustomerId!,
                previousTier, newTierConfig.Tier));

            logger.LogInformation(
                "Customer {CustomerId} upgraded from {OldTier} to {NewTier}.",
                tx.CustomerId, previousTier, newTierConfig.Tier);
        }

        // No SaveChangesAsync here — EventPublisher.FlushAsync persists after all handlers.

        eventPublisher.Enqueue(new PointsEarnedEvent(
            card.Id, e.TenantId, tx.BranchId, tx.CustomerId!,
            finalPoints, card.PointsBalance, e.TransactionId));

        logger.LogInformation(
            "Awarded {Points} loyalty points to card {CardNumber} from transaction {TxNumber}.",
            finalPoints, card.CardNumber, e.TransactionNumber);
    }
}
