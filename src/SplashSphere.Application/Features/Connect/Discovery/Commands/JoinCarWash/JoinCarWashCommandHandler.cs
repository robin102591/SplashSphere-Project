using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Discovery.Commands.JoinCarWash;

public sealed class JoinCarWashCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<JoinCarWashCommand, Result>
{
    private static readonly PlanTier[] EligiblePlans =
        [PlanTier.Trial, PlanTier.Growth, PlanTier.Enterprise];

    public async Task<Result> Handle(JoinCarWashCommand request, CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;

        var user = await db.ConnectUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("ConnectUser", userId));
        }

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId && t.IsActive, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(Error.NotFound("Tenant", request.TenantId));
        }

        var sub = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.TenantId == tenant.Id, cancellationToken);
        if (sub is null || !EligiblePlans.Contains(sub.PlanTier))
        {
            return Result.Failure(Error.Forbidden(
                "This car wash is not available on the Connect app."));
        }

        // Idempotent — if the link already exists, treat as success.
        var existingLink = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                l => l.ConnectUserId == userId && l.TenantId == tenant.Id,
                cancellationToken);
        if (existingLink is not null)
        {
            if (!existingLink.IsActive)
            {
                existingLink.IsActive = true;
                existingLink.LinkedAt = DateTime.UtcNow;
            }
            return Result.Success();
        }

        // Best-effort match to an existing Customer row by phone within this tenant.
        var phone = user.Phone;
        var customer = await db.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                c => c.TenantId == tenant.Id && c.ContactNumber == phone,
                cancellationToken);

        if (customer is null)
        {
            var (firstName, lastName) = SplitName(user.Name);
            customer = new Customer(
                tenantId: tenant.Id,
                firstName: firstName,
                lastName: lastName,
                email: user.Email,
                contactNumber: phone);
            db.Customers.Add(customer);
        }

        var link = new ConnectUserTenantLink(
            connectUserId: userId,
            tenantId: tenant.Id,
            customerId: customer.Id);
        db.ConnectUserTenantLinks.Add(link);

        // UoWBehavior saves.
        return Result.Success();
    }

    /// <summary>
    /// Split a display name into first/last for the Customer record.
    /// Everything before the last space becomes FirstName; the trailing
    /// token becomes LastName. Single-word names go into FirstName.
    /// </summary>
    private static (string FirstName, string LastName) SplitName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0) return ("Connect", "Customer");

        var idx = trimmed.LastIndexOf(' ');
        return idx <= 0
            ? (trimmed, string.Empty)
            : (trimmed[..idx].Trim(), trimmed[(idx + 1)..].Trim());
    }
}
