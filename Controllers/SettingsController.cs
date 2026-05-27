using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Appearance()
    {
        var settings = await GetOrCreateSettingsAsync(User.GetUserId());
        return View(ToViewModel(settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Appearance(AppearanceSettingsViewModel model)
    {
        ValidateSettings(model);

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please choose valid appearance settings.");
            return View(model);
        }

        var settings = await GetOrCreateSettingsAsync(User.GetUserId());
        settings.ThemeMode = model.ThemeMode;
        settings.LayoutStyle = model.LayoutStyle;
        settings.SidebarState = model.SidebarState;
        settings.FontSize = model.FontSize;
        settings.CardStyle = model.CardStyle;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Appearance));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickTheme([FromBody] QuickThemeViewModel model)
    {
        if (!UserSettings.IsValidThemeMode(model.ThemeMode))
        {
            return BadRequest(new { success = false, message = "Choose Light Mode, Dark Mode, or System Default." });
        }

        var settings = await GetOrCreateSettingsAsync(User.GetUserId());
        settings.ThemeMode = model.ThemeMode;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Settings saved successfully." });
    }

    private async Task<UserSettings> GetOrCreateSettingsAsync(int userId)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings != null)
        {
            return settings;
        }

        settings = UserSettings.CreateDefault(userId);
        _context.UserSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    private static AppearanceSettingsViewModel ToViewModel(UserSettings settings)
    {
        return new AppearanceSettingsViewModel
        {
            ThemeMode = settings.ThemeMode,
            LayoutStyle = settings.LayoutStyle,
            SidebarState = settings.SidebarState,
            FontSize = settings.FontSize,
            CardStyle = settings.CardStyle,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private void ValidateSettings(AppearanceSettingsViewModel model)
    {
        if (!UserSettings.IsValidThemeMode(model.ThemeMode))
        {
            ModelState.AddModelError(nameof(model.ThemeMode), "Choose Light Mode, Dark Mode, or System Default.");
        }

        if (!UserSettings.IsValidLayoutStyle(model.LayoutStyle))
        {
            ModelState.AddModelError(nameof(model.LayoutStyle), "Choose Comfortable Layout or Compact Layout.");
        }

        if (!UserSettings.IsValidSidebarState(model.SidebarState))
        {
            ModelState.AddModelError(nameof(model.SidebarState), "Choose Expanded Sidebar or Collapsed Sidebar.");
        }

        if (!UserSettings.IsValidFontSize(model.FontSize))
        {
            ModelState.AddModelError(nameof(model.FontSize), "Choose Small, Medium, or Large font size.");
        }

        if (!UserSettings.IsValidCardStyle(model.CardStyle))
        {
            ModelState.AddModelError(nameof(model.CardStyle), "Choose Default cards or Minimal cards.");
        }
    }
}
