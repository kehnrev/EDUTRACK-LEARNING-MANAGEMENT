using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class AnnouncementsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnnouncementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var announcements = _context.Announcements.Include(a => a.CreatedBy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            announcements = announcements.Where(a => a.Title.Contains(search) || a.Message.Contains(search));
        }

        ViewBag.Search = search;
        return View(await announcements.OrderByDescending(a => a.CreatedAt).ToListAsync());
    }

    [Authorize(Roles = "Admin,Teacher")]
    public IActionResult Create()
    {
        return View(new Announcement());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(Announcement announcement)
    {
        ModelState.Remove(nameof(announcement.CreatedBy));

        if (!ModelState.IsValid)
        {
            return View(announcement);
        }

        announcement.CreatedById = User.GetUserId();
        announcement.CreatedAt = DateTime.UtcNow;

        try
        {
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Announcement posted.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The announcement could not be saved. Please try again.");
            return View(announcement);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);

        if (announcement == null)
        {
            return NotFound();
        }

        if (!CanModify(announcement))
        {
            return Forbid();
        }

        return View(announcement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, Announcement announcement)
    {
        if (id != announcement.AnnouncementId)
        {
            return BadRequest();
        }

        var existing = await _context.Announcements.AsNoTracking().FirstOrDefaultAsync(a => a.AnnouncementId == id);

        if (existing == null)
        {
            return NotFound();
        }

        if (!CanModify(existing))
        {
            return Forbid();
        }

        ModelState.Remove(nameof(announcement.CreatedBy));

        if (!ModelState.IsValid)
        {
            return View(announcement);
        }

        announcement.CreatedById = existing.CreatedById;
        announcement.CreatedAt = existing.CreatedAt;

        try
        {
            _context.Update(announcement);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Announcement updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The announcement could not be updated. Please try again.");
            return View(announcement);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.AnnouncementId == id);

        if (announcement == null)
        {
            return NotFound();
        }

        if (!CanModify(announcement))
        {
            return Forbid();
        }

        return View(announcement);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);

        if (announcement == null)
        {
            return NotFound();
        }

        if (!CanModify(announcement))
        {
            return Forbid();
        }

        _context.Announcements.Remove(announcement);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Announcement deleted.";
        return RedirectToAction(nameof(Index));
    }

    private bool CanModify(Announcement announcement)
    {
        return User.IsInRole("Admin") || announcement.CreatedById == User.GetUserId();
    }
}
