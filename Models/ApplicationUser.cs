using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class ApplicationUser
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Student;

    public bool IsActive { get; set; } = true;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public ICollection<Course> TeachingCourses { get; set; } = new List<Course>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    public UserSettings? Settings { get; set; }
}
