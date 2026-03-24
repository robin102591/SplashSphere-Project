using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Auth.Commands.SetUserPin;

public sealed record SetUserPinCommand(string UserId, string Pin) : ICommand;
