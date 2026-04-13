using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.LogMaintenance;

public sealed class LogMaintenanceCommandHandler(IApplicationDbContext db)
    : IRequestHandler<LogMaintenanceCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        LogMaintenanceCommand request,
        CancellationToken cancellationToken)
    {
        var equipment = await db.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken);

        if (equipment is null)
            return Result.Failure<string>(Error.NotFound("Equipment", request.EquipmentId));

        var log = new MaintenanceLog(request.EquipmentId, request.Type, request.Description, request.PerformedDate)
        {
            Cost = request.Cost,
            PerformedBy = request.PerformedBy,
            NextDueDate = request.NextDueDate,
            NextDueHours = request.NextDueHours,
            Notes = request.Notes,
        };

        db.MaintenanceLogs.Add(log);

        // After maintenance, equipment is operational
        equipment.Status = EquipmentStatus.Operational;

        return Result.Success(log.Id);
    }
}
