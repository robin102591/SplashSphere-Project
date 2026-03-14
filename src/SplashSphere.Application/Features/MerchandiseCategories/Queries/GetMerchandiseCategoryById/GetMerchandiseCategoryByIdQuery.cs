using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategoryById;

public sealed record GetMerchandiseCategoryByIdQuery(string Id) : IQuery<MerchandiseCategoryDto>;
