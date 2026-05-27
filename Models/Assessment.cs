using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Assessment
{
    public int AssessmentId { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Instructions { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Due date")]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Range(1, 1000)]
    [Display(Name = "Total points")]
    public int TotalPoints { get; set; } = 100;

    [Display(Name = "Available offline")]
    public bool IsAvailableOffline { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
