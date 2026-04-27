using System.Text.RegularExpressions;

namespace SplashSphere.Application.Features.Connect;

/// <summary>
/// Normalizes Philippine mobile numbers to canonical E.164 form (<c>+639XXXXXXXXX</c>).
/// Accepts the three shapes users actually type — "09171234567", "639171234567",
/// "+63 917 123 4567" — and rejects everything else. Kept tiny on purpose so it can
/// be reused by auth, booking, and referral features without a separate package.
/// </summary>
public static partial class PhoneNumber
{
    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex PunctuationRegex();

    /// <summary>
    /// Try to normalize <paramref name="input"/> to E.164 PH form.
    /// Returns the canonical string on success, null on failure.
    /// </summary>
    public static string? TryNormalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var digits = PunctuationRegex().Replace(input.Trim(), "");

        if (digits.StartsWith("+63") && digits.Length == 13 && digits[3..].All(char.IsDigit) && digits[3] == '9')
        {
            return digits;
        }

        if (digits.StartsWith("63") && digits.Length == 12 && digits.All(char.IsDigit) && digits[2] == '9')
        {
            return "+" + digits;
        }

        if (digits.StartsWith("09") && digits.Length == 11 && digits.All(char.IsDigit))
        {
            return "+63" + digits[1..];
        }

        return null;
    }

    /// <summary>True if <paramref name="input"/> is a normalizable PH mobile number.</summary>
    public static bool IsValid(string? input) => TryNormalize(input) is not null;
}
