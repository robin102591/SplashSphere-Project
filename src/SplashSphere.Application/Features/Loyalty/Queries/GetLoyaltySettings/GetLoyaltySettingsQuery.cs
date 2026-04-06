using MediatR;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltySettings;

public sealed record GetLoyaltySettingsQuery : IRequest<LoyaltyProgramSettingsDto?>;
