using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Models.Queries.GetModelById;

public sealed record GetModelByIdQuery(string Id) : IQuery<ModelDto>;
