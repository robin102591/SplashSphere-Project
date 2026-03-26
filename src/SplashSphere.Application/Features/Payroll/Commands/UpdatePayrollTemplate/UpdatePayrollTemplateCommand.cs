using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollTemplate;

public sealed record UpdatePayrollTemplateCommand(
    string Id,
    string Name,
    AdjustmentType Type,
    decimal DefaultAmount,
    int SortOrder) : ICommand;
