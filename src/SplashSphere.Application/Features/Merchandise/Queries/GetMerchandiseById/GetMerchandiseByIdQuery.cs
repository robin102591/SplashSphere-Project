using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetMerchandiseById;

public sealed record GetMerchandiseByIdQuery(string Id) : IQuery<MerchandiseDto>;
