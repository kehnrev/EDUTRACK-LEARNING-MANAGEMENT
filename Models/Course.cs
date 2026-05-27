using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Course
{
    public int CourseId { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(600)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(40)]
    [Display(Name = "Grade level")]
    public string GradeLevel { get; set; } = string.Empty;

    [Display(Name = "Teacher")]
    public int TeacherId { get; set; }

    public ApplicationUser? Teacher { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
