using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Makes.Queries.GetMakeById;

public sealed record GetMakeByIdQuery(string Id) : IQuery<MakeDto>;
