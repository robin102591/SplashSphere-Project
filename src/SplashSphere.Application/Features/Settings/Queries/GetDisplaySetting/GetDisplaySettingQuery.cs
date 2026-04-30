using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Queries.GetDisplaySetting;

/// <summary>
/// Gets the display settings that apply for the given branch. Resolution:
/// branch-specific row → tenant default row → in-memory defaults. Pass
/// <c>null</c> for the tenant default.
/// </summary>
public sealed record GetDisplaySettingQuery(string? BranchId = null) : IQuery<DisplaySettingDto>;
