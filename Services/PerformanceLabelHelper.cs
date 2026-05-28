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
            return "Excellent work. Continue to the next lesson.";
        }

        if (percentage >= 75)
        {
            return "Good work. Review minor mistakes.";
        }

        if (percentage >= 60)
        {
            return "Needs improvement. Review the related lesson before the next quiz.";
        }

        return "At Risk. Ask your teacher for support and review the learning material.";
    }
}
