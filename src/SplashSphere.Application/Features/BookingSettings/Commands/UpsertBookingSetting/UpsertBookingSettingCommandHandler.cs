using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.BookingSettings.Commands.UpsertBookingSetting;

public sealed class UpsertBookingSettingCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpsertBookingSettingCommand, Result<BookingSettingDto>>
{
    public async Task<Result<BookingSettingDto>> Handle(
        UpsertBookingSettingCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        // Verify branch belongs to tenant (defense-in-depth beyond query filters).
        var branchExists = await db.Branches
            .AnyAsync(b => b.Id == request.BranchId
                        && b.TenantId == tenantId,
                     cancellationToken);

        if (!branchExists)
            return Result.Failure<BookingSettingDto>(Error.NotFound("Branch", request.BranchId));

        var existing = await db.BookingSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId
                                   && s.BranchId == request.BranchId,
                                 cancellationToken);

        if (existing is null)
        {
            existing = new BookingSetting(tenantId, request.BranchId);
            db.BookingSettings.Add(existing);
        }

        existing.OpenTime              = request.OpenTime;
        existing.CloseTime             = request.CloseTime;
        existing.SlotIntervalMinutes   = request.SlotIntervalMinutes;
        existing.MaxBookingsPerSlot    = request.MaxBookingsPerSlot;
        existing.AdvanceBookingDays    = request.AdvanceBookingDays;
        existing.MinLeadTimeMinutes    = request.MinLeadTimeMinutes;
        existing.NoShowGraceMinutes    = request.NoShowGraceMinutes;
        existing.IsBookingEnabled      = request.IsBookingEnabled;
        existing.ShowInPublicDirectory = request.ShowInPublicDirectory;

        // UnitOfWorkBehavior will SaveChanges.

        return Result.Success(new BookingSettingDto(
            existing.BranchId,
            existing.OpenTime,
            existing.CloseTime,
            existing.SlotIntervalMinutes,
            existing.MaxBookingsPerSlot,
            existing.AdvanceBookingDays,
            existing.MinLeadTimeMinutes,
            existing.NoShowGraceMinutes,
            existing.IsBookingEnabled,
            existing.ShowInPublicDirectory));
    }
}
