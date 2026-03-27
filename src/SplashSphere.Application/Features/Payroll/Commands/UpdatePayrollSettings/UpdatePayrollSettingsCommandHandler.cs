using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
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
        if (request.CutOffStartDay < 0 || request.CutOffStartDay > 6)
            return Result.Failure(Error.Validation("CutOffStartDay must be between 0 and 6."));

        if (request.Frequency is not (1 or 2))
            return Result.Failure(Error.Validation("Frequency must be 1 (Weekly) or 2 (SemiMonthly)."));

        var settings = await db.PayrollSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = new PayrollSettings(tenantContext.TenantId);
            db.PayrollSettings.Add(settings);
        }

        settings.CutOffStartDay = (DayOfWeek)request.CutOffStartDay;
        settings.Frequency = (PayrollFrequency)request.Frequency;
        settings.PayReleaseDayOffset = request.PayReleaseDayOffset;
        settings.AutoCalcGovernmentDeductions = request.AutoCalcGovernmentDeductions;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
