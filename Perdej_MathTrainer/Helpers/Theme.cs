namespace MathTrainer.Helpers;

public static class Theme
{
    public static bool IsDark { get; private set; }

    public static Color Primary       = Color.FromArgb(99, 102, 241);
    public static Color PrimaryDark   = Color.FromArgb(67, 56, 202);
    public static Color Background    = Color.FromArgb(249, 250, 251);
    public static Color CardBg        = Color.FromArgb(255, 255, 255);
    public static Color TextPrimary   = Color.FromArgb(17, 24, 39);
    public static Color TextSecondary = Color.FromArgb(107, 114, 128);
    public static Color Border        = Color.FromArgb(229, 231, 235);
    public static Color Success       = Color.FromArgb(34, 197, 94);
    public static Color Error         = Color.FromArgb(239, 68, 68);
    public static Color Warning       = Color.FromArgb(245, 158, 11);
    public static Color AccentHover   = Color.FromArgb(238, 242, 255);

    public static Font HeaderFont => new("Segoe UI", 14, FontStyle.Bold);
    public static Font BodyFont   => new("Segoe UI", 11, FontStyle.Regular);
    public static Font SmallFont  => new("Segoe UI", 9,  FontStyle.Regular);

    public static void ApplyTheme(bool isDark)
    {
        IsDark = isDark;
        if (isDark)
        {
            Background    = Color.FromArgb(17, 24, 39);
            CardBg        = Color.FromArgb(31, 41, 55);
            TextPrimary   = Color.FromArgb(243, 244, 246);
            TextSecondary = Color.FromArgb(156, 163, 175);
            Border        = Color.FromArgb(55, 65, 81);
            AccentHover   = Color.FromArgb(55, 65, 81);
        }
        else
        {
            Primary       = Color.FromArgb(99, 102, 241);
            PrimaryDark   = Color.FromArgb(67, 56, 202);
            Background    = Color.FromArgb(249, 250, 251);
            CardBg        = Color.FromArgb(255, 255, 255);
            TextPrimary   = Color.FromArgb(17, 24, 39);
            TextSecondary = Color.FromArgb(107, 114, 128);
            Border        = Color.FromArgb(229, 231, 235);
            Success       = Color.FromArgb(34, 197, 94);
            Error         = Color.FromArgb(239, 68, 68);
            Warning       = Color.FromArgb(245, 158, 11);
            AccentHover   = Color.FromArgb(238, 242, 255);
        }
    }
}
