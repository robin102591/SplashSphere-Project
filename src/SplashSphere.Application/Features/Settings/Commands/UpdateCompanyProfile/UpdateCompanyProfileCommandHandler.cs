using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateCompanyProfile;

public sealed class UpdateCompanyProfileCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<UpdateCompanyProfileCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCompanyProfileCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant", tenantContext.TenantId));

        // ── Identity ─────────────────────────────────────────────────────────
        tenant.Name    = request.Name;
        tenant.Tagline = request.Tagline;

        // ── Contact ──────────────────────────────────────────────────────────
        tenant.Email         = request.Email;
        tenant.ContactNumber = request.ContactNumber;
        tenant.Website       = request.Website;

        // ── Structured address ────────────────────────────────────────────────
        tenant.StreetAddress = request.StreetAddress;
        tenant.Barangay      = request.Barangay;
        tenant.City          = request.City;
        tenant.Province      = request.Province;
        tenant.ZipCode       = request.ZipCode;

        // Keep the legacy single-string `Address` in sync with the structured
        // fields. Several handlers (Auth/me, Billing PDF, Connect listings,
        // Franchise detail) still read `Address`; this avoids a churn-heavy
        // migration of those readers in slice 1.
        var derived = ComposeAddress(tenant.StreetAddress, tenant.Barangay, tenant.City, tenant.Province, tenant.ZipCode);
        if (!string.IsNullOrWhiteSpace(derived))
            tenant.Address = derived;

        // ── Tax & registration ────────────────────────────────────────────────
        tenant.TaxId             = request.TaxId;
        tenant.BusinessPermitNo  = request.BusinessPermitNo;
        tenant.IsVatRegistered   = request.IsVatRegistered;

        // ── Social & payment ──────────────────────────────────────────────────
        tenant.FacebookUrl     = request.FacebookUrl;
        tenant.InstagramHandle = request.InstagramHandle;
        tenant.GCashNumber     = request.GCashNumber;
        // Brand color: normalize empty/whitespace to null and uppercase the hex
        // so equality comparisons (and the customer-display CSS variable) are
        // stable regardless of how the user typed it.
        tenant.PrimaryColorHex = string.IsNullOrWhiteSpace(request.PrimaryColorHex)
            ? null
            : request.PrimaryColorHex.ToUpperInvariant();

        // UnitOfWorkBehavior commits.
        return Result.Success();
    }

    /// <summary>
    /// Renders the structured address parts into the comma-separated single
    /// string consumed by legacy readers. Returns empty when no parts are set
    /// (so the existing <see cref="Domain.Entities.Tenant.Address"/> stays).
    /// </summary>
    private static string ComposeAddress(string? street, string? barangay, string? city, string? province, string? zip)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(street))   parts.Add(street.Trim());
        if (!string.IsNullOrWhiteSpace(barangay)) parts.Add(barangay.Trim());
        if (!string.IsNullOrWhiteSpace(city))     parts.Add(city.Trim());
        if (!string.IsNullOrWhiteSpace(province)) parts.Add(province.Trim());
        if (!string.IsNullOrWhiteSpace(zip))      parts.Add(zip.Trim());
        return string.Join(", ", parts);
    }
}
