using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PerformanceAnalyticsService _analytics;

    public TeacherController(ApplicationDbContext context, PerformanceAnalyticsService analytics)
    {
        _context = context;
        _analytics = analytics;
    }

    public async Task<IActionResult> Index()
    {
        var teacherId = User.GetUserId();
        var courses = await _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.Title)
            .ToListAsync();
        var courseIds = courses.Select(c => c.CourseId).ToList();
        var classAverageByCourse = new Dictionary<string, decimal>();

        foreach (var course in courses)
        {
            classAverageByCourse[course.Title] = await _analytics.GetCourseAverageAsync(course.CourseId);
        }

        var expectedSubmissions = 0;

        foreach (var course in courses)
        {
            var enrolled = await _context.Enrollments.CountAsync(e => e.CourseId == course.CourseId);
            var assessments = await _context.Assessments.CountAsync(a => a.CourseId == course.CourseId);
            expectedSubmissions += enrolled * assessments;
        }

        var actualSubmissions = await _context.Submissions
            .CountAsync(s => s.Assessment != null && courseIds.Contains(s.Assessment.CourseId));

        var model = new TeacherDashboardViewModel
        {
            MyCourses = courseIds.Count,
            TotalStudentsEnrolled = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync(),
            PendingSubmissions = await _context.Assessments
                .Where(a => courseIds.Contains(a.CourseId))
                .CountAsync(a => !_context.Submissions.Any(s => s.AssessmentId == a.AssessmentId)),
            AverageClassScore = await _analytics.GetTeacherAverageAsync(teacherId),
            StudentsNeedingImprovement = await _analytics.GetStudentsNeedingImprovementAsync(teacherId),
            RecentAssessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderByDescending(a => a.DueDate)
                .Take(5)
                .ToListAsync(),
            ClassAverageByCourse = classAverageByCourse,
            SubmissionStatusSummary = new Dictionary<string, int>
            {
                ["Submitted"] = actualSubmissions,
                ["Missing"] = Math.Max(0, expectedSubmissions - actualSubmissions)
            }
        };

        return View(model);
    }
}
