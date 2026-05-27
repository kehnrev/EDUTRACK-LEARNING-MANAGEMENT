using EduTrackAnalytics.Data;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class SubmissionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? assessmentId, string? search)
    {
        var submissions = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .AsQueryable();

        if (assessmentId.HasValue)
        {
            submissions = submissions.Where(s => s.AssessmentId == assessmentId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            submissions = submissions.Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            submissions = submissions.Where(s => s.StudentId == User.GetUserId());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            submissions = submissions.Where(s =>
                s.Student != null && s.Student.FullName.Contains(search) ||
                s.Assessment != null && s.Assessment.Title.Contains(search));
        }

        ViewBag.Search = search;
        ViewBag.AssessmentId = assessmentId;
        return View(await submissions.OrderByDescending(s => s.SubmittedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var submission = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .Include(s => s.StudentAnswers)
            .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);

        if (submission == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Student") && submission.StudentId != User.GetUserId())
        {
            return Forbid();
        }

        if (User.IsInRole("Teacher") && submission.Assessment?.Course?.TeacherId != User.GetUserId())
        {
            return Forbid();
        }

        return View(submission);
    }
}
