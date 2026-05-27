using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.ViewModels;

public class AssessmentTakeViewModel
{
    public int AssessmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsAvailableOffline { get; set; }
    public List<TakeQuestionViewModel> Questions { get; set; } = new();
}

public class TakeQuestionViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose an answer before submitting.")]
    public string? SelectedAnswer { get; set; }
}

public class OfflineSubmissionDto
{
    public int AssessmentId { get; set; }
    public List<OfflineAnswerDto> Answers { get; set; } = new();
}

public class OfflineAnswerDto
{
    public int QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
}
