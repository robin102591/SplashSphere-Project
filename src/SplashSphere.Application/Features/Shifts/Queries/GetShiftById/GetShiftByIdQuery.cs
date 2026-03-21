using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftById;

public sealed record GetShiftByIdQuery(string ShiftId) : IQuery<ShiftDetailDto?>;
