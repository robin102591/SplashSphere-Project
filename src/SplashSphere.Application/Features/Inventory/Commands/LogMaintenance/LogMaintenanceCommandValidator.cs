using FluentValidation;

namespace SplashSphere.Application.Features.Inventory.Commands.LogMaintenance;

public sealed class LogMaintenanceCommandValidator : AbstractValidator<LogMaintenanceCommand>
{
    public LogMaintenanceCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1024);
    }
}
