using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly PerformanceAnalyticsService _analytics;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        PerformanceAnalyticsService analytics,
        ILogger<AdminController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _analytics = analytics;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var settings = await _context.UserSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == User.GetUserId()) ?? UserSettings.CreateDefault(User.GetUserId());

            var enrollmentRows = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .OrderBy(c => c.Title)
                .Select(c => new
                {
                    c.Title,
                    EnrollmentCount = c.Enrollments.Count
                })
                .ToListAsync();

            var enrollmentSummary = enrollmentRows
                .GroupBy(c => c.Title)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.EnrollmentCount));

            var passed = await _context.Submissions.CountAsync(s => s.Score >= settings.DefaultPassingScore);
            var needsSupport = await _context.Submissions.CountAsync(s => s.Score < settings.DefaultPassingScore);

            var model = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student),
                TotalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalAssessments = await _context.Assessments.CountAsync(),
                OverallAveragePerformance = await _analytics.GetOverallAverageAsync(),
                PendingSyncSubmissions = await _context.Submissions.CountAsync(s => s.Status == SubmissionStatus.PendingSync),
                RecentAnnouncements = await _context.Announcements
                    .AsNoTracking()
                    .Include(a => a.CreatedBy)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentUsers = await _context.Users
                    .AsNoTracking()
                    .OrderByDescending(u => u.DateCreated)
                    .Take(5)
                    .ToListAsync(),
                RecentSubmissions = await _context.Submissions
                    .AsNoTracking()
                    .Include(s => s.Student)
                    .Include(s => s.Assessment)
                    .ThenInclude(a => a!.Course)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(5)
                    .ToListAsync(),
                CourseEnrollmentSummary = enrollmentSummary,
                PassFailDistribution = new Dictionary<string, int>
                {
                    ["Passed"] = passed,
                    ["Needs Support"] = needsSupport
                }
            };

            ViewBag.ShowDemoDataNotice = settings.EnableDemoDataNotice;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin dashboard data could not be loaded.");
            Console.Error.WriteLine($"Admin dashboard data could not be loaded. Details: {ex.Message}");
            TempData["Error"] = "Admin dashboard data could not be loaded. Please try again.";
            ViewBag.ShowDemoDataNotice = false;

            return View(CreateEmptyDashboardModel());
        }
    }

    private static AdminDashboardViewModel CreateEmptyDashboardModel()
    {
        return new AdminDashboardViewModel
        {
            RecentAnnouncements = new List<Announcement>(),
            RecentUsers = new List<ApplicationUser>(),
            RecentSubmissions = new List<Submission>(),
            CourseEnrollmentSummary = new Dictionary<string, int>(),
            PassFailDistribution = new Dictionary<string, int>
            {
                ["Passed"] = 0,
                ["Needs Support"] = 0
            }
        };
    }

    public async Task<IActionResult> Users(string? search, UserRole? role)
    {
        var users = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            users = users.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        if (role.HasValue)
        {
            users = users.Where(u => u.Role == role.Value);
        }

        ViewBag.Search = search;
        ViewBag.Role = role;
        return View(await users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync());
    }

    public IActionResult CreateUser()
    {
        return View("UserForm", new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "A password is required for new users.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View("UserForm", model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            Role = model.Role,
            IsActive = model.IsActive,
            DateCreated = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password!);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User account created.";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The account could not be saved. Please try again.");
            return View("UserForm", model);
        }
    }

    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return View("UserForm", new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(UserFormViewModel model)
    {
        if (model.Id == null)
        {
            return BadRequest();
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id.Value))
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View("UserForm", model);
        }

        var user = await _context.Users.FindAsync(model.Id.Value);

        if (user == null)
        {
            return NotFound();
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Role = model.Role;
        user.IsActive = model.IsActive;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The account could not be updated. Please try again.");
            return View("UserForm", model);
        }
    }

    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? NotFound() : View(user);
    }

    [HttpPost, ActionName("DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        if (id == User.GetUserId())
        {
            TempData["Error"] = "You cannot delete your own active administrator account.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User account deleted.";
        }
        catch (Exception)
        {
            _context.Entry(user).State = EntityState.Modified;
            user.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Error"] = "This user has related records, so the account was deactivated instead.";
        }

        return RedirectToAction(nameof(Users));
    }
}
