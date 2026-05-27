using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class UserSettings
{
    public const string DefaultThemeMode = "Light";
    public const string DefaultLayoutStyle = "Comfortable";
    public const string DefaultSidebarState = "Expanded";
    public const string DefaultFontSize = "Medium";
    public const string DefaultCardStyle = "Default";

    public static readonly string[] ThemeModes = ["Light", "Dark", "System"];
    public static readonly string[] LayoutStyles = ["Comfortable", "Compact"];
    public static readonly string[] SidebarStates = ["Expanded", "Collapsed"];
    public static readonly string[] FontSizes = ["Small", "Medium", "Large"];
    public static readonly string[] CardStyles = ["Default", "Minimal"];

    public int UserSettingsId { get; set; }

    public int UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [Required, StringLength(20)]
    public string ThemeMode { get; set; } = DefaultThemeMode;

    [Required, StringLength(20)]
    public string LayoutStyle { get; set; } = DefaultLayoutStyle;

    [Required, StringLength(20)]
    public string SidebarState { get; set; } = DefaultSidebarState;

    [Required, StringLength(20)]
    public string FontSize { get; set; } = DefaultFontSize;

    [Required, StringLength(20)]
    public string CardStyle { get; set; } = DefaultCardStyle;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static UserSettings CreateDefault(int userId)
    {
        return new UserSettings
        {
            UserId = userId,
            ThemeMode = DefaultThemeMode,
            LayoutStyle = DefaultLayoutStyle,
            SidebarState = DefaultSidebarState,
            FontSize = DefaultFontSize,
            CardStyle = DefaultCardStyle,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static bool IsValidThemeMode(string? value) => ThemeModes.Contains(value);
    public static bool IsValidLayoutStyle(string? value) => LayoutStyles.Contains(value);
    public static bool IsValidSidebarState(string? value) => SidebarStates.Contains(value);
    public static bool IsValidFontSize(string? value) => FontSizes.Contains(value);
    public static bool IsValidCardStyle(string? value) => CardStyles.Contains(value);
}
