using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.ViewModels;

public class AppearanceSettingsViewModel
{
    [Required]
    [Display(Name = "Theme mode")]
    public string ThemeMode { get; set; } = "Light";

    [Required]
    [Display(Name = "Layout style")]
    public string LayoutStyle { get; set; } = "Comfortable";

    [Required]
    [Display(Name = "Sidebar preference")]
    public string SidebarState { get; set; } = "Expanded";

    [Required]
    [Display(Name = "Font size")]
    public string FontSize { get; set; } = "Medium";

    [Required]
    [Display(Name = "Dashboard card style")]
    public string CardStyle { get; set; } = "Default";

    public DateTime? UpdatedAt { get; set; }
}

public class QuickThemeViewModel
{
    public string ThemeMode { get; set; } = "Light";
}
