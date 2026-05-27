using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? courseId, string? search)
    {
        var enrollments = _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c!.Teacher)
            .Include(e => e.Student)
            .AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            enrollments = enrollments.Where(e => e.Course != null && e.Course.TeacherId == teacherId);
        }

        if (courseId.HasValue)
        {
            enrollments = enrollments.Where(e => e.CourseId == courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            enrollments = enrollments.Where(e =>
                e.Student != null && e.Student.FullName.Contains(search) ||
                e.Course != null && e.Course.Title.Contains(search));
        }

        ViewBag.CourseId = courseId;
        ViewBag.Search = search;
        return View(await enrollments.OrderBy(e => e.Course!.Title).ThenBy(e => e.Student!.FullName).ToListAsync());
    }

    public async Task<IActionResult> Create(int? courseId)
    {
        await LoadSelectListsAsync(courseId);
        return View(new Enrollment { CourseId = courseId ?? 0, EnrollmentDate = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Enrollment enrollment)
    {
        if (!await CanModifyCourseAsync(enrollment.CourseId))
        {
            return Forbid();
        }

        if (await _context.Enrollments.AnyAsync(e => e.CourseId == enrollment.CourseId && e.StudentId == enrollment.StudentId))
        {
            ModelState.AddModelError(nameof(enrollment.StudentId), "This student is already enrolled in the selected course.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync(enrollment.CourseId, enrollment.StudentId);
            return View(enrollment);
        }

        enrollment.EnrollmentDate = DateTime.UtcNow;

        try
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Student enrolled.";
            return RedirectToAction(nameof(Index), new { enrollment.CourseId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The enrollment could not be saved. Please try again.");
            await LoadSelectListsAsync(enrollment.CourseId, enrollment.StudentId);
            return View(enrollment);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.EnrollmentId == id);

        if (enrollment == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(enrollment.CourseId))
        {
            return Forbid();
        }

        return View(enrollment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);

        if (enrollment == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(enrollment.CourseId))
        {
            return Forbid();
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Enrollment removed.";
        return RedirectToAction(nameof(Index), new { enrollment.CourseId });
    }

    private async Task<bool> CanModifyCourseAsync(int courseId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var teacherId = User.GetUserId();
        return await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);
    }

    private async Task LoadSelectListsAsync(int? selectedCourseId = null, int? selectedStudentId = null)
    {
        var courses = _context.Courses.OrderBy(c => c.Title).AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            courses = courses.Where(c => c.TeacherId == teacherId);
        }

        ViewBag.Courses = new SelectList(await courses.ToListAsync(), "CourseId", "Title", selectedCourseId);
        ViewBag.Students = new SelectList(
            await _context.Users
                .Where(u => u.Role == UserRole.Student && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(),
            "Id",
            "FullName",
            selectedStudentId);
    }
}
