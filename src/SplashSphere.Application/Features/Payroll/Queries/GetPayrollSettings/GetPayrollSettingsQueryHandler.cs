using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollSettings;

public sealed class GetPayrollSettingsQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetPayrollSettingsQuery, PayrollSettingsDto>
{
    public async Task<PayrollSettingsDto> Handle(
        GetPayrollSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        // If a branch is specified, try branch-specific first
        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            var branchSettings = await db.PayrollSettings
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId && s.BranchId == request.BranchId)
                .Select(s => new { s.CutOffStartDay, s.Frequency, s.PayReleaseDayOffset, s.AutoCalcGovernmentDeductions, s.BranchId, BranchName = s.Branch != null ? s.Branch.Name : null })
                .FirstOrDefaultAsync(cancellationToken);

            if (branchSettings is not null)
                return new PayrollSettingsDto(
                    (int)branchSettings.CutOffStartDay,
                    (int)branchSettings.Frequency,
                    branchSettings.PayReleaseDayOffset,
                    branchSettings.AutoCalcGovernmentDeductions,
                    branchSettings.BranchId,
                    branchSettings.BranchName,
                    IsInherited: false);

            // Fall back to tenant default, marked as inherited
            var tenantDefault = await db.PayrollSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.BranchId == null, cancellationToken);

            return tenantDefault is null
                ? new PayrollSettingsDto(1, 1, 3, false, null, null, IsInherited: true)
                : new PayrollSettingsDto(
                    (int)tenantDefault.CutOffStartDay,
                    (int)tenantDefault.Frequency,
                    tenantDefault.PayReleaseDayOffset,
                    tenantDefault.AutoCalcGovernmentDeductions,
                    null, null,
                    IsInherited: true);
        }

        // No branch specified — return tenant default
        var settings = await db.PayrollSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.BranchId == null, cancellationToken);

        return settings is null
            ? new PayrollSettingsDto(1, 1, 3, false)
            : new PayrollSettingsDto(
                (int)settings.CutOffStartDay,
                (int)settings.Frequency,
                settings.PayReleaseDayOffset,
                settings.AutoCalcGovernmentDeductions);
    }
}
