using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.GetCarWashDetail;

/// <summary>
/// Detail view for a single car wash tenant — tenant overview, publicly listed
/// branches, and the active service catalogue. Returns null when the tenant is
/// ineligible for discovery (inactive, wrong plan, or no public branches).
/// </summary>
public sealed record GetCarWashDetailQuery(string TenantId) : IQuery<CarWashDetailDto?>;
