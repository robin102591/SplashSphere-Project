using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Commands.DeleteLogo;

/// <summary>
/// Removes the current tenant's logo URLs and tries to delete the
/// underlying R2 objects. Idempotent — calling on a tenant without a
/// logo is a no-op success.
/// </summary>
public sealed record DeleteLogoCommand : ICommand;
