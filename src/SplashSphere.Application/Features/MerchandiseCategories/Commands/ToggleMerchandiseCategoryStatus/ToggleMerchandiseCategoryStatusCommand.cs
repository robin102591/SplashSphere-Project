using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Commands.ToggleMerchandiseCategoryStatus;

public sealed record ToggleMerchandiseCategoryStatusCommand(string Id) : ICommand;
