using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Display.Queries.GetDisplayConfig;

/// <summary>
/// Returns the resolved render config for a customer-facing display:
/// display settings (with branch-fallback) + company branding.
/// </summary>
public sealed record GetDisplayConfigQuery(string? BranchId = null) : IQuery<DisplayConfigDto>;
