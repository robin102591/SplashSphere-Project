using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.UpdateMerchandiseCategory;

public sealed record UpdateMerchandiseCategoryCommand(
    string Id,
    string Name,
    string? Description) : ICommand;
