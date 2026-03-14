using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Models.Commands.CreateModel;

public sealed record CreateModelCommand(string MakeId, string Name) : ICommand<string>;
