using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Commands.CancelCashAdvance;

public sealed record CancelCashAdvanceCommand(string CashAdvanceId) : ICommand;
