namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised after the onboarding wizard completes: Clerk Organization created,
/// <c>Tenant</c> record persisted (id = Clerk org ID), first <c>Branch</c> created,
/// and the onboarding <c>User</c> linked to the tenant.
/// Consumed by: welcome email dispatch, initial seed data trigger (dev only),
/// audit log.
/// </summary>
public sealed record TenantOnboardedEvent(
    string TenantId,
    string TenantName,
    string UserId,
    string FirstBranchId,
    string FirstBranchName
) : DomainEventBase;
