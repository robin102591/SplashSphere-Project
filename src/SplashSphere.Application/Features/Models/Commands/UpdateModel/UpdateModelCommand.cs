using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.UpdateModel;

/// <summary>MakeId is immutable — only the model name can change.</summary>
public sealed record UpdateModelCommand(string Id, string Name) : ICommand;
