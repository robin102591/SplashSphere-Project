using Clerk.BackendAPI;
using Clerk.BackendAPI.Models.Operations;
using Microsoft.Extensions.Configuration;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Calls the Clerk Backend API to create organizations and manage memberships.
/// Instantiates a <see cref="ClerkBackendApi"/> client per-call using the secret key
/// from configuration (stateless — no SDK singleton needed).
/// </summary>
public sealed class ClerkOrganizationService(IConfiguration configuration) : IClerkOrganizationService
{
    public async Task<string> CreateOrganizationAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var sdk = CreateClient();

        var response = await sdk.Organizations.CreateAsync(
            new CreateOrganizationRequestBody { Name = name });

        if (response.Organization is null)
            throw new InvalidOperationException(
                "Clerk returned a null organization after creation.");

        return response.Organization.Id;
    }

    public async Task AddMemberAsync(
        string organizationId,
        string clerkUserId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var sdk = CreateClient();

        await sdk.OrganizationMemberships.CreateAsync(
            organizationId: organizationId,
            requestBody: new CreateOrganizationMembershipRequestBody
            {
                UserId = clerkUserId,
                Role   = role,
            });
    }

    public async Task InviteMemberAsync(
        string organizationId,
        string emailAddress,
        string inviterClerkUserId,
        string role = "org:member",
        string? redirectUrl = null,
        CancellationToken cancellationToken = default)
    {
        var sdk = CreateClient();

        await sdk.OrganizationInvitations.CreateAsync(
            organizationId: organizationId,
            requestBody: new CreateOrganizationInvitationRequestBody
            {
                EmailAddress = emailAddress,
                InviterUserId = inviterClerkUserId,
                Role = role,
                RedirectUrl = redirectUrl,
            });
    }

    private ClerkBackendApi CreateClient()
    {
        var secretKey = configuration["Clerk:SecretKey"]
            ?? throw new InvalidOperationException("Clerk:SecretKey is not configured.");

        return new ClerkBackendApi(bearerAuth: secretKey);
    }
}
