using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Commands.ApproveCashAdvance;

public sealed record ApproveCashAdvanceCommand(string CashAdvanceId) : ICommand;
