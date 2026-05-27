using EduTrackAnalytics.Models;

namespace EduTrackAnalytics.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalAssessments { get; set; }
    public decimal OverallAveragePerformance { get; set; }
    public int PendingSyncSubmissions { get; set; }
    public List<Announcement> RecentAnnouncements { get; set; } = new();
    public Dictionary<string, int> CourseEnrollmentSummary { get; set; } = new();
    public Dictionary<string, int> PassFailDistribution { get; set; } = new();
}

public class TeacherDashboardViewModel
{
    public int MyCourses { get; set; }
    public int TotalStudentsEnrolled { get; set; }
    public int PendingSubmissions { get; set; }
    public decimal AverageClassScore { get; set; }
    public List<ApplicationUser> StudentsNeedingImprovement { get; set; } = new();
    public List<Assessment> RecentAssessments { get; set; } = new();
    public Dictionary<string, decimal> ClassAverageByCourse { get; set; } = new();
    public Dictionary<string, int> SubmissionStatusSummary { get; set; } = new();
}

public class StudentDashboardViewModel
{
    public int EnrolledCourses { get; set; }
    public int UpcomingAssessments { get; set; }
    public decimal AverageScore { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int OfflineAvailableLessons { get; set; }
    public int PendingSyncSubmissions { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
    public List<Submission> RecentScores { get; set; } = new();
    public Dictionary<string, decimal> CourseProgress { get; set; } = new();
    public Dictionary<string, decimal> RecentAssessmentScores { get; set; } = new();
    public int CompletedAssessments { get; set; }
    public int RemainingAssessments { get; set; }
}

public class OfflineLibraryViewModel
{
    public List<Lesson> Lessons { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
}
