namespace EduTrackAnalytics.Services;

public static class ChartLabelHelper
{
    private static readonly Dictionary<string, string> CourseLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["General Mathematics"] = "Gen Math",
        ["Earth and Life Science"] = "ELS",
        ["Oral Communication"] = "Oral Comm",
        ["Empowerment Technologies"] = "EmpTech",
        ["Practical Research 2"] = "Research 2",
        ["Media and Information Literacy"] = "MIL",
        ["Entrepreneurship"] = "Entrep",
        ["Contemporary Philippine Arts"] = "CPAR"
    };

    public static string GetShortCourseLabel(string title)
    {
        return CourseLabels.TryGetValue(title, out var label) ? label : title;
    }

    public static string GetShortAssessmentLabel(string title)
    {
        return title
            .Replace("General Mathematics", "Gen Math", StringComparison.OrdinalIgnoreCase)
            .Replace("Earth and Life Science", "ELS", StringComparison.OrdinalIgnoreCase)
            .Replace("Oral Communication", "Oral Comm", StringComparison.OrdinalIgnoreCase)
            .Replace("Empowerment Technologies", "EmpTech", StringComparison.OrdinalIgnoreCase)
            .Replace("Practical Research 2", "Research 2", StringComparison.OrdinalIgnoreCase)
            .Replace("Media and Information Literacy", "MIL", StringComparison.OrdinalIgnoreCase)
            .Replace("Entrepreneurship", "Entrep", StringComparison.OrdinalIgnoreCase)
            .Replace("Contemporary Philippine Arts", "CPAR", StringComparison.OrdinalIgnoreCase)
            .Replace("Foundations Quiz", "Foundations", StringComparison.OrdinalIgnoreCase)
            .Replace("Application Quiz", "Application", StringComparison.OrdinalIgnoreCase);
    }
}
