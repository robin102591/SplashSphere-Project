using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.MarkRoyaltyPaid;

public sealed record MarkRoyaltyPaidCommand(string RoyaltyPeriodId, string? PaymentReference) : ICommand;
