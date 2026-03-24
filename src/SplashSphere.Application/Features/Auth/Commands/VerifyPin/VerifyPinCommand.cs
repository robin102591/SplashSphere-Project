using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Auth.Commands.VerifyPin;

public sealed record VerifyPinCommand(string Pin) : ICommand<bool>;
