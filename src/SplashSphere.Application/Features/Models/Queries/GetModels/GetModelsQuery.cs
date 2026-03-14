using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Queries.GetModels;

public sealed record GetModelsQuery(
    int Page = 1,
    int PageSize = 20,
    string? MakeId = null,
    string? Search = null) : IQuery<PagedResult<ModelDto>>;
