using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Sizes.Queries.GetSizeById;

public sealed record GetSizeByIdQuery(string Id) : IQuery<SizeDto>;
