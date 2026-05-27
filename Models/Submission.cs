using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Submission
{
    public int SubmissionId { get; set; }

    [Display(Name = "Assessment")]
    public int AssessmentId { get; set; }

    public Assessment? Assessment { get; set; }

    [Display(Name = "Student")]
    public int StudentId { get; set; }

    public ApplicationUser? Student { get; set; }

    [Range(0, 1000)]
    public decimal Score { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Synced;

    public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}
