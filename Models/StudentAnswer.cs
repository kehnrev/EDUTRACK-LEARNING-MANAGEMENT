using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class StudentAnswer
{
    public int StudentAnswerId { get; set; }

    public int SubmissionId { get; set; }

    public Submission? Submission { get; set; }

    public int QuestionId { get; set; }

    public Question? Question { get; set; }

    [Required, RegularExpression("A|B|C|D", ErrorMessage = "Selected answer must be A, B, C, or D.")]
    [Display(Name = "Selected answer")]
    public string SelectedAnswer { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    [Range(0, 100)]
    public decimal PointsEarned { get; set; }
}
