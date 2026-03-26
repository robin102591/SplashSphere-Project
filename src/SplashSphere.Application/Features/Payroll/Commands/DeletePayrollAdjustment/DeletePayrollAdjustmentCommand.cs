using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.DeletePayrollAdjustment;

public sealed record DeletePayrollAdjustmentCommand(string AdjustmentId) : ICommand;
