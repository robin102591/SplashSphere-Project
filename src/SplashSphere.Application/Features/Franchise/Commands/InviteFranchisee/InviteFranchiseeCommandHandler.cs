using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.InviteFranchisee;

public sealed class InviteFranchiseeCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEmailService emailService)
    : IRequestHandler<InviteFranchiseeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        InviteFranchiseeCommand request,
        CancellationToken cancellationToken)
    {
        // ── Guard: caller must be a Franchisor ──────────────────────────────
        var tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null || tenant.TenantType != TenantType.Franchisor)
            return Result.Failure<string>(Error.Forbidden(
                "Only Franchisor tenants can send franchise invitations."));

        // ── Check for existing unused invitation to the same email ──────────
        var existing = await context.FranchiseInvitations
            .IgnoreQueryFilters()
            .AnyAsync(i => i.FranchisorTenantId == tenantContext.TenantId
                        && i.Email == request.Email
                        && !i.IsUsed
                        && i.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (existing)
            return Result.Failure<string>(Error.Validation(
                "An active invitation already exists for this email address."));

        // ── Generate 64-character hex token (256-bit entropy) ───────────────
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        // ── Create invitation ───────────────────────────────────────────────
        var invitation = new FranchiseInvitation(
            tenantContext.TenantId,
            request.Email,
            request.BusinessName,
            token)
        {
            OwnerName = request.OwnerName,
            FranchiseCode = request.FranchiseCode,
            TerritoryName = request.TerritoryName,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        context.FranchiseInvitations.Add(invitation);

        // ── Send invitation email ───────────────────────────────────────────
        var acceptUrl = $"https://app.splashsphere.ph/franchise/accept?token={token}";
        var htmlBody = $"""
            <h2>Franchise Invitation from {tenant.Name}</h2>
            <p>Hello{(string.IsNullOrWhiteSpace(request.OwnerName) ? "" : $" {request.OwnerName}")},</p>
            <p><strong>{tenant.Name}</strong> has invited you to join their franchise network as a franchisee.</p>
            {(string.IsNullOrWhiteSpace(request.TerritoryName) ? "" : $"<p><strong>Territory:</strong> {request.TerritoryName}</p>")}
            <p>Click the link below to accept this invitation and set up your franchise:</p>
            <p><a href="{acceptUrl}" style="display:inline-block;padding:12px 24px;background-color:#2563eb;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">Accept Invitation</a></p>
            <p>This invitation expires in 7 days.</p>
            <p>If you did not expect this invitation, you can safely ignore this email.</p>
            """;

        await emailService.SendAsync(new EmailMessage(
            request.Email,
            $"Franchise Invitation from {tenant.Name}",
            htmlBody,
            $"You've been invited to join {tenant.Name}'s franchise network. Accept here: {acceptUrl}"),
            cancellationToken);

        return Result.Success(invitation.Id);
    }
}
