using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Branches.Queries.GetBranchById;

/// <summary>Returns a single branch. Throws NotFoundException if not found.</summary>
public sealed record GetBranchByIdQuery(string Id) : IQuery<BranchDto>;
