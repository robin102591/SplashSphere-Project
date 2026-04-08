using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto?>
{
    public async Task<CurrentUserDto?> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        // Must bypass global query filter — the user may have no TenantId yet.
        var user = await context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.ClerkUserId == tenantContext.ClerkUserId)
            .Select(u => new
            {
                u.Id,
                u.ClerkUserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.IsActive,
                HasPin = u.PinHash != null,
                u.TenantId,
                TenantName            = u.Tenant != null ? u.Tenant.Name            : null,
                TenantEmail           = u.Tenant != null ? u.Tenant.Email           : null,
                TenantContactNumber   = u.Tenant != null ? u.Tenant.ContactNumber   : null,
                TenantAddress         = u.Tenant != null ? u.Tenant.Address         : null,
                TenantIsActive        = u.Tenant != null ? (bool?)u.Tenant.IsActive  : null,
                TenantType            = u.Tenant != null ? (int?)u.Tenant.TenantType : null,
                TenantParentId        = u.Tenant != null ? u.Tenant.ParentTenantId  : null,
                TenantFranchiseCode   = u.Tenant != null ? u.Tenant.FranchiseCode   : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return null;

        var tenant = user.TenantId is not null
            ? new CurrentUserTenantDto(
                user.TenantId,
                user.TenantName!,
                user.TenantEmail!,
                user.TenantContactNumber!,
                user.TenantAddress!,
                user.TenantIsActive!.Value,
                user.TenantType ?? 0,
                user.TenantParentId,
                user.TenantFranchiseCode)
            : null;

        return new CurrentUserDto(
            user.Id,
            user.ClerkUserId,
            user.Email,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Role,
            user.IsActive,
            user.HasPin,
            tenant);
    }
}
