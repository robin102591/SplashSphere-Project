using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PosStations.Queries.GetPosStationById;

public sealed record GetPosStationByIdQuery(string Id) : IQuery<PosStationDto>;
