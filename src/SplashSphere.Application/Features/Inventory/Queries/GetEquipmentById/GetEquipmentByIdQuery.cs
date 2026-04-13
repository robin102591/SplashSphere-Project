using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipmentById;

public sealed record GetEquipmentByIdQuery(string Id) : IQuery<EquipmentDetailDto?>;
