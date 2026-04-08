using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.PushServiceTemplates;

public sealed record PushServiceTemplatesCommand(string FranchiseeTenantId) : ICommand;
