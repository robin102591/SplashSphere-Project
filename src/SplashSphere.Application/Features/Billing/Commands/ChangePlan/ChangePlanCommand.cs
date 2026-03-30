using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Billing.Commands.ChangePlan;

public sealed record ChangePlanCommand(PlanTier NewPlan) : ICommand;
