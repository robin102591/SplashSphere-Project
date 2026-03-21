using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.UpdateShiftSettings;

public sealed class UpdateShiftSettingsCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpdateShiftSettingsCommand, Result>
{
    public async Task<Result> Handle(
        UpdateShiftSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await db.ShiftSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        if (settings is null)
        {
            settings = new ShiftSettings(tenantContext.TenantId);
            db.ShiftSettings.Add(settings);
        }

        settings.DefaultOpeningFund          = request.DefaultOpeningFund;
        settings.AutoApproveThreshold        = request.AutoApproveThreshold;
        settings.FlagThreshold               = request.FlagThreshold;
        settings.RequireShiftForTransactions = request.RequireShiftForTransactions;
        settings.EndOfDayReminderTime        = request.EndOfDayReminderTime;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
