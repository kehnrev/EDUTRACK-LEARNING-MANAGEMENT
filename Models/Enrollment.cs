using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Display(Name = "Student")]
    public int StudentId { get; set; }

    public ApplicationUser? Student { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    [Display(Name = "Completion")]
    public int CompletionPercentage { get; set; }
}
