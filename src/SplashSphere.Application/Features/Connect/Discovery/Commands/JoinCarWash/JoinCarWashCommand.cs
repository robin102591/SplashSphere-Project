using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Discovery.Commands.JoinCarWash;

/// <summary>
/// Link the authenticated Connect user to the tenant identified by
/// <paramref name="TenantId"/>. The handler best-effort matches an existing
/// <c>Customer</c> by phone number; otherwise it creates a new Customer using
/// the Connect profile name. Idempotent — no-op when the link already exists.
/// </summary>
public sealed record JoinCarWashCommand(string TenantId) : ICommand;
