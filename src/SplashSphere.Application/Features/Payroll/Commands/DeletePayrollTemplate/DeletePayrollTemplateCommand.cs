using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.DeletePayrollTemplate;

public sealed record DeletePayrollTemplateCommand(string Id) : ICommand;
