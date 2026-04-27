using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Loyalty.Commands.RedeemReward;

/// <summary>
/// Deduct points for a reward at a tenant and emit a <c>PointTransaction</c>
/// of type <c>Redeemed</c>. The customer presents the returned transaction ID
/// (or a QR derived from it) at the POS to claim the benefit.
/// </summary>
public sealed record RedeemRewardCommand(
    string TenantId,
    string RewardId) : ICommand<ConnectRedemptionResultDto>;
