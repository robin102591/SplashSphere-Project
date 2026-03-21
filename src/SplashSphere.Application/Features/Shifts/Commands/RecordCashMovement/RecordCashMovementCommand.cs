using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.RecordCashMovement;

/// <summary>Records a manual cash-in or cash-out movement during an open shift. Returns the movement ID.</summary>
public sealed record RecordCashMovementCommand(
    string ShiftId,
    CashMovementType Type,
    decimal Amount,
    string Reason,
    string? Reference) : ICommand<string>;
