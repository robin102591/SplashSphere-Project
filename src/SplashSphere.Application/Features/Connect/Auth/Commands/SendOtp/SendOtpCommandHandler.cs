using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Auth.Commands.SendOtp;

public sealed class SendOtpCommandHandler(
    IOtpSender sender,
    IOtpStore store)
    : IRequestHandler<SendOtpCommand, Result<SendOtpResponse>>
{
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(5);

    public async Task<Result<SendOtpResponse>> Handle(
        SendOtpCommand request,
        CancellationToken cancellationToken)
    {
        var phone = PhoneNumber.TryNormalize(request.PhoneNumber);
        if (phone is null)
        {
            return Result.Failure<SendOtpResponse>(
                Error.Validation("Phone number must be a valid Philippine mobile number."));
        }

        var rate = await store.TryRegisterSendAsync(phone, cancellationToken);
        if (!rate.Allowed)
        {
            return Result.Failure<SendOtpResponse>(new Error("OTP_RATE_LIMIT",
                rate.Reason ?? "Too many OTP requests — please try again later."));
        }

        var code = await sender.SendAsync(phone, cancellationToken);
        await store.SaveCodeAsync(phone, code, OtpTtl, cancellationToken);

        return Result.Success(new SendOtpResponse((int)OtpTtl.TotalSeconds));
    }
}
