using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description) : ICommand<string>;
