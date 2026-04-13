using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateEquipmentStatus;

public sealed record UpdateEquipmentStatusCommand(string Id, EquipmentStatus Status) : ICommand;
