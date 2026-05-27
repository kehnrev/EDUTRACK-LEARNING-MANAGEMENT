using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private static readonly string[] SeniorHighGradeLevels = ["Grade 11", "Grade 12"];
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? subject, string? gradeLevel)
    {
        var courses = _context.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
            .Where(c => SeniorHighGradeLevels.Contains(c.GradeLevel))
            .AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            courses = courses.Where(c => c.TeacherId == User.GetUserId());
        }
        else if (User.IsInRole("Student"))
        {
            var studentId = User.GetUserId();
            courses = courses.Where(c => c.Enrollments.Any(e => e.StudentId == studentId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            courses = courses.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(subject))
        {
            courses = courses.Where(c => c.Subject == subject);
        }

        if (!string.IsNullOrWhiteSpace(gradeLevel))
        {
            courses = courses.Where(c => c.GradeLevel == gradeLevel);
        }

        ViewBag.Search = search;
        ViewBag.Subject = subject;
        ViewBag.GradeLevel = gradeLevel;
        ViewBag.Subjects = await _context.Courses
            .Where(c => SeniorHighGradeLevels.Contains(c.GradeLevel))
            .Select(c => c.Subject)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
        ViewBag.GradeLevels = SeniorHighGradeLevels;

        return View(await courses.OrderBy(c => c.Title).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Lessons.OrderByDescending(l => l.DateCreated))
            .Include(c => c.Assessments.OrderBy(a => a.DueDate))
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        if (!await CanViewCourseAsync(course))
        {
            return Forbid();
        }

        return View(course);
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create()
    {
        await LoadTeachersAsync();
        return View(new Course());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(Course course)
    {
        if (User.IsInRole("Teacher"))
        {
            course.TeacherId = User.GetUserId();
        }
        else if (course.TeacherId == 0)
        {
            ModelState.AddModelError(nameof(course.TeacherId), "Choose a teacher for this course.");
        }

        if (await _context.Courses.AnyAsync(c =>
                c.Title == course.Title && c.Subject == course.Subject && c.TeacherId == course.TeacherId))
        {
            ModelState.AddModelError(nameof(course.Title), "This teacher already has a course with the same title and subject.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTeachersAsync(course.TeacherId);
            return View(course);
        }

        course.DateCreated = DateTime.UtcNow;

        try
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course created.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The course could not be saved. Please try again.");
            await LoadTeachersAsync(course.TeacherId);
            return View(course);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        if (!CanModifyCourse(course))
        {
            return Forbid();
        }

        await LoadTeachersAsync(course.TeacherId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        if (id != course.CourseId)
        {
            return BadRequest();
        }

        var existing = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == id);

        if (existing == null)
        {
            return NotFound();
        }

        if (!CanModifyCourse(existing))
        {
            return Forbid();
        }

        if (User.IsInRole("Teacher"))
        {
            course.TeacherId = User.GetUserId();
        }
        else if (course.TeacherId == 0)
        {
            ModelState.AddModelError(nameof(course.TeacherId), "Choose a teacher for this course.");
        }

        if (await _context.Courses.AnyAsync(c =>
                c.CourseId != id &&
                c.Title == course.Title &&
                c.Subject == course.Subject &&
                c.TeacherId == course.TeacherId))
        {
            ModelState.AddModelError(nameof(course.Title), "Another course already uses this title and subject for the selected teacher.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTeachersAsync(course.TeacherId);
            return View(course);
        }

        course.DateCreated = existing.DateCreated;

        try
        {
            _context.Update(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The course could not be updated. Please try again.");
            await LoadTeachersAsync(course.TeacherId);
            return View(course);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        if (!CanModifyCourse(course))
        {
            return Forbid();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        if (!CanModifyCourse(course))
        {
            return Forbid();
        }

        try
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course deleted.";
        }
        catch (Exception)
        {
            TempData["Error"] = "The course has related records and could not be deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CanModifyCourse(Course course)
    {
        return User.IsInRole("Admin") || course.TeacherId == User.GetUserId();
    }

    private async Task<bool> CanViewCourseAsync(Course course)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Teacher") && course.TeacherId == User.GetUserId())
        {
            return true;
        }

        var studentId = User.GetUserId();
        return await _context.Enrollments.AnyAsync(e => e.CourseId == course.CourseId && e.StudentId == studentId);
    }

    private async Task LoadTeachersAsync(int? selectedTeacherId = null)
    {
        ViewBag.Teachers = new SelectList(
            await _context.Users
                .Where(u => u.Role == UserRole.Teacher && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync(),
            "Id",
            "FullName",
            selectedTeacherId);
    }
}
