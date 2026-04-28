using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Queries.GetReceiptSetting;

/// <summary>
/// Gets the receipt-design settings that apply for the given branch.
/// Resolution: branch-specific row → tenant default row → create-on-read.
/// Pass <c>null</c> for the tenant default (slice 2 default flow).
/// </summary>
public sealed record GetReceiptSettingQuery(string? BranchId = null) : IQuery<ReceiptSettingDto>;
