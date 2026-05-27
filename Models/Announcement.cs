using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Announcement
{
    public int AnnouncementId { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Created by")]
    public int CreatedById { get; set; }

    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
