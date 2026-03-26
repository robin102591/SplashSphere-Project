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
        var settings = await db.PayrollSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        // Return defaults if no settings record exists yet (Monday = 1, Weekly = 1)
        return settings is null
            ? new PayrollSettingsDto(1, 1)
            : new PayrollSettingsDto((int)settings.CutOffStartDay, (int)settings.Frequency);
    }
}
