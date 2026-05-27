using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Question
{
    public int QuestionId { get; set; }

    [Display(Name = "Assessment")]
    public int AssessmentId { get; set; }

    public Assessment? Assessment { get; set; }

    [Required, StringLength(1200)]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionA { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionB { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionC { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string OptionD { get; set; } = string.Empty;

    [Required, RegularExpression("A|B|C|D", ErrorMessage = "Correct answer must be A, B, C, or D.")]
    [Display(Name = "Correct answer")]
    public string CorrectAnswer { get; set; } = "A";

    [Range(1, 100)]
    public int Points { get; set; } = 10;
}
