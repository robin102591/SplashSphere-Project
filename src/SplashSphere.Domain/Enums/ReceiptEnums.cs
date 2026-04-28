namespace SplashSphere.Domain.Enums;

// ── Receipt format / layout ─────────────────────────────────────────────────

/// <summary>
/// Logo size on a printed receipt. Controls the rendered image height in
/// the receipt header.
/// </summary>
public enum LogoSize
{
    Small  = 0,
    Medium = 1,
    Large  = 2,
}

/// <summary>
/// Horizontal alignment of the logo on a printed receipt.
/// </summary>
public enum LogoPosition
{
    Left   = 0,
    Center = 1,
}

/// <summary>
/// Physical paper width of the thermal printer the receipt is rendered for.
/// 58mm ≈ 164 points; 80mm ≈ 226 points.
/// </summary>
public enum ReceiptWidth
{
    Mm58 = 0,
    Mm80 = 1,
}

/// <summary>
/// Base font size for the receipt body. Header/total lines render larger
/// regardless of this setting.
/// </summary>
public enum ReceiptFontSize
{
    Small  = 0,
    Normal = 1,
    Large  = 2,
}
