using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetServiceTemplates;

public sealed record GetServiceTemplatesQuery : IQuery<IReadOnlyList<FranchiseServiceTemplateDto>>;
