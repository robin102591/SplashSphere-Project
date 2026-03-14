using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    string Id,
    string Name,
    string? Description) : ICommand;
