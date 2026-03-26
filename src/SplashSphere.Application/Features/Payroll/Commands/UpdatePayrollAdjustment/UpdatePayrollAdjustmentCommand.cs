using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollAdjustment;

public sealed record UpdatePayrollAdjustmentCommand(
    string AdjustmentId,
    decimal Amount,
    string? Notes) : ICommand;
