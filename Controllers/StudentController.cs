using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PerformanceAnalyticsService _analytics;

    public StudentController(ApplicationDbContext context, PerformanceAnalyticsService analytics)
    {
        _context = context;
        _analytics = analytics;
    }

    public async Task<IActionResult> Index()
    {
        var studentId = User.GetUserId();
        var courses = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c!.Lessons)
            .Where(e => e.StudentId == studentId)
            .Select(e => e.Course!)
            .OrderBy(c => c.Title)
            .ToListAsync();

        var courseIds = courses.Select(c => c.CourseId).ToList();
        var submittedAssessmentIds = await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .Select(s => s.AssessmentId)
            .Distinct()
            .ToListAsync();

        var upcoming = await _context.Assessments
            .Include(a => a.Course)
            .Where(a => courseIds.Contains(a.CourseId) && a.DueDate >= DateTime.UtcNow && !submittedAssessmentIds.Contains(a.AssessmentId))
            .OrderBy(a => a.DueDate)
            .Take(5)
            .ToListAsync();

        var recentScores = await _context.Submissions
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5)
            .ToListAsync();

        var progress = await _analytics.GetStudentProgressAsync(studentId);
        var averageProgress = progress.Count == 0 ? 0 : progress.Values.Average();
        var totalAssessments = await _context.Assessments.CountAsync(a => courseIds.Contains(a.CourseId));
        var completedAssessments = submittedAssessmentIds.Count;

        var model = new StudentDashboardViewModel
        {
            EnrolledCourses = courses.Count,
            Courses = courses,
            UpcomingAssessments = upcoming.Count,
            Assessments = upcoming,
            RecentScores = recentScores,
            AverageScore = await _context.Submissions
                .Where(s => s.StudentId == studentId)
                .Select(s => (decimal?)s.Score)
                .AverageAsync() ?? 0,
            ProgressPercentage = Math.Round(averageProgress, 2),
            OfflineAvailableLessons = courses.SelectMany(c => c.Lessons).Count(l => l.IsAvailableOffline),
            PendingSyncSubmissions = await _context.Submissions.CountAsync(s => s.StudentId == studentId && s.Status == SubmissionStatus.PendingSync),
            CourseProgress = progress,
            RecentAssessmentScores = recentScores
                .OrderBy(s => s.SubmittedAt)
                .GroupBy(s => s.Assessment?.Title ?? $"Assessment {s.AssessmentId}")
                .ToDictionary(g => g.Key, g => g.Last().Score),
            CompletedAssessments = completedAssessments,
            RemainingAssessments = Math.Max(0, totalAssessments - completedAssessments)
        };

        return View(model);
    }

    public async Task<IActionResult> OfflineLibrary()
    {
        var studentId = User.GetUserId();
        var courseIds = await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseId)
            .ToListAsync();

        var model = new OfflineLibraryViewModel
        {
            Lessons = await _context.Lessons
                .Include(l => l.Course)
                .Where(l => courseIds.Contains(l.CourseId))
                .OrderBy(l => l.Course!.Title)
                .ThenBy(l => l.Title)
                .ToListAsync(),
            Assessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderBy(a => a.Course!.Title)
                .ThenBy(a => a.Title)
                .ToListAsync()
        };

        return View(model);
    }
}
