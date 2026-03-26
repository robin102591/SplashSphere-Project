using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Onboarding.Commands.CreateOnboarding;

public sealed class CreateOnboardingCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IClerkOrganizationService clerkService)
    : IRequestHandler<CreateOnboardingCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateOnboardingCommand request,
        CancellationToken cancellationToken)
    {
        // ── Load user (bypassing global filter — no TenantId yet) ─────────────
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ClerkUserId == tenantContext.ClerkUserId, cancellationToken);

        if (user is null)
            return Result.Failure<string>(Error.Validation(
                "User record not found. Ensure your account was created before onboarding."));

        if (!string.IsNullOrWhiteSpace(user.TenantId))
            return Result.Failure<string>(Error.Validation(
                "This user has already completed onboarding."));

        // ── Create Clerk Organization ─────────────────────────────────────────
        string orgId;
        try
        {
            orgId = await clerkService.CreateOrganizationAsync(request.BusinessName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation(
                $"Failed to create Clerk Organization: {ex.Message}"));
        }

        // ── Add user as org:admin in Clerk ────────────────────────────────────
        try
        {
            await clerkService.AddMemberAsync(
                orgId,
                tenantContext.ClerkUserId,
                "org:admin",
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation(
                $"Failed to add user to Clerk Organization: {ex.Message}"));
        }

        // ── Create Tenant (id = Clerk org id) ─────────────────────────────────
        var tenant = new Tenant(orgId, request.BusinessName, request.BusinessEmail,
            request.ContactNumber, request.Address);
        context.Tenants.Add(tenant);

        // ── Create first Branch ───────────────────────────────────────────────
        var branch = new Branch(orgId, request.BranchName, request.BranchCode,
            request.BranchAddress, request.BranchContactNumber);
        context.Branches.Add(branch);

        // ── Link User to Tenant ───────────────────────────────────────────────
        user.TenantId = orgId;
        user.Role     = "org:admin";

        // ── Pre-seed government deduction templates ─────────────────────────
        var governmentTemplates = new[]
        {
            new PayrollAdjustmentTemplate(orgId, "SSS", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 1 },
            new PayrollAdjustmentTemplate(orgId, "PhilHealth", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 2 },
            new PayrollAdjustmentTemplate(orgId, "Pag-IBIG", AdjustmentType.Deduction, 100m)
                { IsSystemDefault = true, SortOrder = 3 },
            new PayrollAdjustmentTemplate(orgId, "Tax (BIR Withholding)", AdjustmentType.Deduction, 0m)
                { IsSystemDefault = true, SortOrder = 4 },
        };
        context.PayrollAdjustmentTemplates.AddRange(governmentTemplates);

        // Populate TenantContext so downstream UnitOfWork / query filters work
        // correctly if any subsequent command in this request needs them.
        tenantContext.TenantId = orgId;

        return Result.Success(orgId);
    }
}
