namespace EduTrackAnalytics.ViewModels;

public class ReportSummaryViewModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime DateGenerated { get; set; } = DateTime.Now;
    public decimal AverageScore { get; set; }
    public int TotalRows { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
}
