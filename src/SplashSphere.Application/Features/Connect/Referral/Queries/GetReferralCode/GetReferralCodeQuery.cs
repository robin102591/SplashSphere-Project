using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Referral.Queries.GetReferralCode;

/// <summary>
/// Read the authenticated customer's referral code at a tenant. Lazily issues
/// a code on first read — so a joined customer always has something to share.
/// <para>
/// Failure cases: caller not signed in, tenant doesn't offer loyalty, caller
/// has not joined the tenant.
/// </para>
/// </summary>
public sealed record GetReferralCodeQuery(string TenantId) : ICommand<ConnectReferralCodeDto>;
