using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;

public sealed class UpdatePayrollSettingsCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpdatePayrollSettingsCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePayrollSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await db.PayrollSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = new PayrollSettings(tenantContext.TenantId);
            db.PayrollSettings.Add(settings);
        }

        settings.CutOffStartDay = (DayOfWeek)request.CutOffStartDay;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
