using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.CreateMerchandiseCategory;

public sealed record CreateMerchandiseCategoryCommand(
    string Name,
    string? Description) : ICommand<string>;
