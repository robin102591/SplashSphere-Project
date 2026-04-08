using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.UpsertServiceTemplate;

public sealed record UpsertServiceTemplateCommand(
    string? Id,
    string ServiceName,
    string? Description,
    string? CategoryName,
    decimal BasePrice,
    int DurationMinutes,
    bool IsRequired,
    string? PricingMatrixJson,
    string? CommissionMatrixJson) : ICommand<string>;
