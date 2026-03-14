using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Onboarding.Commands.CreateOnboarding;

/// <summary>
/// Runs the full tenant-creation flow for a brand-new user:
/// 1. Creates a Clerk Organization.
/// 2. Adds the current user as org:admin.
/// 3. Creates the Tenant row (id = Clerk org id).
/// 4. Creates the first Branch.
/// 5. Links the User to the Tenant (sets User.TenantId).
/// Returns the new TenantId on success.
/// </summary>
public sealed record CreateOnboardingCommand(
    string BusinessName,
    string BusinessEmail,
    string ContactNumber,
    string Address,
    string BranchName,
    string BranchCode,
    string BranchAddress,
    string BranchContactNumber) : ICommand<string>;
