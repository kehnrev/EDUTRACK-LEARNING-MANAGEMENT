using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Services;

public class PerformanceAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public PerformanceAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetOverallAverageAsync()
    {
        return await _context.Submissions.Select(s => (decimal?)s.Score).AverageAsync() ?? 0;
    }

    public async Task<decimal> GetTeacherAverageAsync(int teacherId)
    {
        return await _context.Submissions
            .Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId)
            .Select(s => (decimal?)s.Score)
            .AverageAsync() ?? 0;
    }

    public async Task<decimal> GetCourseAverageAsync(int courseId)
    {
        return await _context.Submissions
            .Where(s => s.Assessment != null && s.Assessment.CourseId == courseId)
            .Select(s => (decimal?)s.Score)
            .AverageAsync() ?? 0;
    }

    public async Task<List<ApplicationUser>> GetStudentsNeedingImprovementAsync(int? teacherId = null, int? courseId = null, decimal threshold = 75)
    {
        var enrollmentQuery = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsQueryable();

        if (teacherId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.Course != null && e.Course.TeacherId == teacherId.Value);
        }

        if (courseId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.CourseId == courseId.Value);
        }

        var studentIds = await enrollmentQuery.Select(e => e.StudentId).Distinct().ToListAsync();
        var students = await _context.Users
            .Where(u => studentIds.Contains(u.Id) && u.Role == UserRole.Student)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<ApplicationUser>();

        foreach (var student in students)
        {
            var submissions = _context.Submissions.Where(s => s.StudentId == student.Id);
            var enrolledCourses = _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Select(e => e.Course!);
            var lowProgressEnrollments = _context.Enrollments.Where(e => e.StudentId == student.Id && e.CompletionPercentage < 60);

            if (teacherId.HasValue)
            {
                submissions = submissions.Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId.Value);
                enrolledCourses = enrolledCourses.Where(c => c.TeacherId == teacherId.Value);
                lowProgressEnrollments = lowProgressEnrollments.Where(e => e.Course != null && e.Course.TeacherId == teacherId.Value);
            }

            if (courseId.HasValue)
            {
                submissions = submissions.Where(s => s.Assessment != null && s.Assessment.CourseId == courseId.Value);
                enrolledCourses = enrolledCourses.Where(c => c.CourseId == courseId.Value);
                lowProgressEnrollments = lowProgressEnrollments.Where(e => e.CourseId == courseId.Value);
            }

            var average = await submissions.Select(s => (decimal?)s.Score).AverageAsync();
            var courseIds = await enrolledCourses.Select(c => c.CourseId).Distinct().ToListAsync();
            var missingWork = await _context.Assessments
                .Where(a => courseIds.Contains(a.CourseId))
                .CountAsync(a => !_context.Submissions.Any(s => s.AssessmentId == a.AssessmentId && s.StudentId == student.Id));
            var lowProgress = await lowProgressEnrollments.AnyAsync();

            if (!average.HasValue || average.Value < threshold || missingWork > 0 || lowProgress)
            {
                result.Add(student);
            }
        }

        return result;
    }

    public async Task<Dictionary<string, decimal>> GetStudentProgressAsync(int studentId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderBy(e => e.Course!.Title)
            .ToListAsync();

        var progress = new Dictionary<string, decimal>();

        foreach (var enrollment in enrollments)
        {
            var totalAssessments = await _context.Assessments.CountAsync(a => a.CourseId == enrollment.CourseId);
            var completedAssessments = await _context.Submissions
                .Where(s => s.StudentId == studentId && s.Assessment != null && s.Assessment.CourseId == enrollment.CourseId)
                .Select(s => s.AssessmentId)
                .Distinct()
                .CountAsync();

            var assessmentProgress = totalAssessments == 0
                ? enrollment.CompletionPercentage
                : Math.Round((decimal)completedAssessments / totalAssessments * 100, 2);
            var calculated = totalAssessments == 0
                ? enrollment.CompletionPercentage
                : Math.Round((assessmentProgress + enrollment.CompletionPercentage) / 2, 2);

            progress[enrollment.Course?.Title ?? "Course"] = calculated;
        }

        return progress;
    }

    public async Task<int> GetCourseCompletionRateAsync(int courseId)
    {
        var completions = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => (int?)e.CompletionPercentage)
            .AverageAsync();

        return Convert.ToInt32(completions ?? 0);
    }
}
