using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.ViewModels;

public class ContactViewModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(140)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Message { get; set; } = string.Empty;
}
