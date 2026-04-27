using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Referral.Commands.ApplyReferral;

/// <summary>
/// Apply a referral code at a specific tenant. Records the caller as the
/// <c>ReferredCustomerId</c> on a matching Pending referral row. Points are
/// <b>not</b> awarded yet — that happens only when the referred customer
/// completes their first transaction (handled downstream in 22.4).
/// <para>
/// Caller must have already joined the tenant (link exists). The same
/// customer cannot use their own code, and each customer can be the referee
/// at most once per tenant.
/// </para>
/// </summary>
public sealed record ApplyReferralCommand(string TenantId, string Code)
    : ICommand<ApplyReferralResultDto>;
