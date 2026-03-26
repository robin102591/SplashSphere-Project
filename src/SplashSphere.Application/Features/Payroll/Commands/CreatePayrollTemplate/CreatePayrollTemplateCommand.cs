using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollTemplate;

public sealed record CreatePayrollTemplateCommand(
    string Name,
    AdjustmentType Type,
    decimal DefaultAmount) : ICommand<string>;
