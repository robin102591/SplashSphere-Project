using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CreateCashAdvance;

public sealed record CreateCashAdvanceCommand(
    string EmployeeId,
    decimal Amount,
    decimal DeductionPerPeriod,
    string? Reason) : ICommand<string>;
