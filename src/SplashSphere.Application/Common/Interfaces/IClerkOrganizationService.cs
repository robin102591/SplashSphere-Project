namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstracts Clerk Backend API calls needed during tenant onboarding.
/// Implemented in Infrastructure so the Application layer stays SDK-free.
/// </summary>
public interface IClerkOrganizationService
{
    /// <summary>
    /// Creates a new Clerk Organization with the given name.
    /// Returns the new organization's Clerk ID (used as TenantId).
    /// </summary>
    Task<string> CreateOrganizationAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a Clerk user as a member of an organization with the specified role.
    /// </summary>
    Task AddMemberAsync(
        string organizationId,
        string clerkUserId,
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a Clerk organization invitation email to the specified address.
    /// The recipient will receive an email with a link to accept and join the organization.
    /// </summary>
    Task InviteMemberAsync(
        string organizationId,
        string emailAddress,
        string inviterClerkUserId,
        string role = "org:member",
        string? redirectUrl = null,
        CancellationToken cancellationToken = default);
}
