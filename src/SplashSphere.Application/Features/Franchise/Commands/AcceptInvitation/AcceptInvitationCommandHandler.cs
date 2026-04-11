using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IClerkOrganizationService clerkService)
    : IRequestHandler<AcceptInvitationCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Validate token ───────────────────────────────────────────────
        var invitation = await context.FranchiseInvitations
            .IgnoreQueryFilters()
            .Include(i => i.FranchisorTenant)
                .ThenInclude(t => t.FranchiseSettings)
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invitation is null)
            return Result.Failure<string>(Error.Validation("Invalid invitation token."));

        if (invitation.IsUsed)
            return Result.Failure<string>(Error.Validation("This invitation has already been used."));

        if (invitation.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure<string>(Error.Validation("This invitation has expired."));

        // ── 2. Ensure current user hasn't already onboarded ─────────────────
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.ClerkUserId == tenantContext.ClerkUserId, cancellationToken);

        if (user is null)
            return Result.Failure<string>(Error.Validation(
                "User record not found. Ensure your account was created before accepting."));

        if (!string.IsNullOrWhiteSpace(user.TenantId))
            return Result.Failure<string>(Error.Validation(
                "This user already belongs to a tenant. Cannot accept franchise invitation."));

        // ── 3. Create Clerk Organization ────────────────────────────────────
        string orgId;
        try
        {
            orgId = await clerkService.CreateOrganizationAsync(
                request.BusinessName, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Validation(
                $"Failed to create Clerk Organization: {ex.Message}"));
        }

        // ── 4. Add user as org:admin in Clerk ───────────────────────────────
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

        // ── 5. Create Tenant (Franchisee, linked to Franchisor) ─────────────
        var tenant = new Tenant(orgId, request.BusinessName, request.Email,
            request.ContactNumber, request.Address)
        {
            TenantType = TenantType.Franchisee,
            ParentTenantId = invitation.FranchisorTenantId,
            FranchiseCode = invitation.FranchiseCode,
        };
        context.Tenants.Add(tenant);

        // ── 6. Create first Branch ──────────────────────────────────────────
        var branch = new Branch(orgId, request.BranchName, request.BranchCode,
            request.BranchAddress, request.BranchContactNumber);
        context.Branches.Add(branch);

        // ── 7. Link User to Tenant ──────────────────────────────────────────
        user.TenantId = orgId;
        user.Role = "org:admin";

        // ── 8. Create Trial subscription ────────────────────────────────────
        var now = DateTime.UtcNow;
        var subscription = new TenantSubscription(orgId, PlanTier.Trial, SubscriptionStatus.Trial)
        {
            TrialStartDate = now,
            TrialEndDate = now.AddDays(14),
            SmsCountResetDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        context.TenantSubscriptions.Add(subscription);

        // ── 9. Pre-seed government deduction templates ──────────────────────
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

        // ── 10. Pre-seed default expense categories ─────────────────────────
        var defaultCategories = new[]
        {
            "Water Bill", "Electricity", "Rent", "Soap & Chemicals",
            "Equipment Maintenance", "Employee Meals/Snacks", "Transportation",
            "Supplies (towels, sponges)", "Miscellaneous", "Insurance", "Taxes & Permits"
        };
        foreach (var name in defaultCategories)
            context.ExpenseCategories.Add(new ExpenseCategory(orgId, name));

        // ── 10b. Pre-seed reference data ────────────────────────────────────
        var vehicleTypeNames = new[] { "Sedan", "SUV", "Van", "Truck", "Hatchback", "Pickup", "Motorcycle" };
        foreach (var vtName in vehicleTypeNames)
            context.VehicleTypes.Add(new VehicleType(orgId, vtName));

        var sizeNames = new[] { "Small", "Medium", "Large", "XL" };
        foreach (var sName in sizeNames)
            context.Sizes.Add(new Size(orgId, sName));

        var makeModels = new Dictionary<string, string[]>
        {
            ["Toyota"] = ["Vios", "Innova", "Fortuner", "Hilux", "Wigo", "Avanza"],
            ["Honda"] = ["City", "Civic", "CR-V", "BR-V", "Jazz"],
            ["Mitsubishi"] = ["Mirage", "Xpander", "Montero Sport", "Strada"],
            ["Nissan"] = ["Almera", "Navara", "Terra", "Kicks"],
            ["Suzuki"] = ["Ertiga", "Swift", "Celerio", "Jimny", "Dzire"],
            ["Ford"] = ["EcoSport", "Territory", "Ranger", "Everest"],
        };
        foreach (var (makeName, models) in makeModels)
        {
            var make = new Make(orgId, makeName);
            context.Makes.Add(make);
            foreach (var modelName in models)
                context.Models.Add(new Model(orgId, make.Id, modelName));
        }

        var serviceCategories = new[] { "Basic Services", "Premium Services", "Add-Ons", "Detailing" };
        foreach (var scName in serviceCategories)
            context.ServiceCategories.Add(new ServiceCategory(orgId, scName));

        var merchCategories = new[]
        {
            "Cleaning Chemicals", "Wax & Polish", "Tire & Trim",
            "Towels & Cloths", "Brushes & Tools", "Packaging & Miscellaneous"
        };
        foreach (var mcName in merchCategories)
            context.MerchandiseCategories.Add(new MerchandiseCategory(orgId, mcName));

        // ── 11. Clone service templates if enforced ─────────────────────────
        var settings = invitation.FranchisorTenant.FranchiseSettings;
        if (settings is { EnforceStandardServices: true })
        {
            var templates = await context.FranchiseServiceTemplates
                .Where(t => t.FranchisorTenantId == invitation.FranchisorTenantId && t.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (templates.Count > 0)
            {
                var category = new ServiceCategory(orgId, "Standard Services");
                context.ServiceCategories.Add(category);

                foreach (var tmpl in templates)
                {
                    var service = new Service(orgId, category.Id, tmpl.ServiceName,
                        tmpl.BasePrice, tmpl.Description)
                    {
                        IsActive = true,
                    };
                    context.Services.Add(service);
                }
            }
        }

        // ── 12. Create FranchiseAgreement (Active) ──────────────────────────
        var agreement = new FranchiseAgreement(
            invitation.FranchisorTenantId,
            orgId,
            invitation.TerritoryName ?? "")
        {
            AgreementNumber = $"FA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}",
            StartDate = DateTime.UtcNow,
            Status = AgreementStatus.Active,
        };
        context.FranchiseAgreements.Add(agreement);

        // ── 13. Mark invitation as used ─────────────────────────────────────
        invitation.IsUsed = true;
        invitation.AcceptedByTenantId = orgId;

        // Populate TenantContext for downstream operations
        tenantContext.TenantId = orgId;

        return Result.Success(orgId);
    }
}
