using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.CashAdvances.Commands.DisburseCashAdvance;

public sealed record DisburseCashAdvanceCommand(string CashAdvanceId) : ICommand;
