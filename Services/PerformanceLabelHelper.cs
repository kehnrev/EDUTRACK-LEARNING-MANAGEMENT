namespace EduTrackAnalytics.Services;

public static class PerformanceLabelHelper
{
    public static string GetLabel(decimal percentage)
    {
        if (percentage >= 85)
        {
            return "Excellent";
        }

        if (percentage >= 75)
        {
            return "Good";
        }

        if (percentage >= 60)
        {
            return "Needs Improvement";
        }

        return "At Risk";
    }

    public static string GetBadgeClass(decimal percentage)
    {
        if (percentage >= 85)
        {
            return "text-bg-success";
        }

        if (percentage >= 75)
        {
            return "text-bg-primary";
        }

        if (percentage >= 60)
        {
            return "text-bg-warning";
        }

        return "text-bg-danger";
    }

    public static string GetRecommendation(decimal percentage)
    {
        if (percentage >= 85)
        {
            return "Keep up the strong work and continue practicing with the next activity.";
        }

        if (percentage >= 75)
        {
            return "Good progress. Review the missed questions before the next quiz.";
        }

        if (percentage >= 60)
        {
            return "Review the related lesson and ask your teacher for a short follow-up activity.";
        }

        return "Please revisit the lesson materials and ask your teacher for academic support.";
    }
}
