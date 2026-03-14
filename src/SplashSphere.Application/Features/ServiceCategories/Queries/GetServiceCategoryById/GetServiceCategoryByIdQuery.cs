using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategoryById;

public sealed record GetServiceCategoryByIdQuery(string Id) : IQuery<ServiceCategoryDto>;
