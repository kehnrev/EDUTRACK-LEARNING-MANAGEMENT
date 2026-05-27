using EduTrackAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(120);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(c => new { c.Title, c.Subject, c.TeacherId }).IsUnique();
            entity.HasOne(c => c.Teacher)
                .WithMany(u => u.TeachingCourses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Assessment>()
            .HasOne(a => a.Course)
            .WithMany(c => c.Assessments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Assessment)
            .WithMany(a => a.Questions)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.Score).HasColumnType("decimal(18,2)");
            entity.HasOne(s => s.Assessment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.Property(a => a.PointsEarned).HasColumnType("decimal(18,2)");
            entity.HasOne(a => a.Submission)
                .WithMany(s => s.StudentAnswers)
                .HasForeignKey(a => a.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.CreatedBy)
            .WithMany(u => u.Announcements)
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.CourseId, e.StudentId }).IsUnique();
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasIndex(s => s.UserId).IsUnique();
            entity.Property(s => s.ThemeMode).HasMaxLength(20);
            entity.Property(s => s.LayoutStyle).HasMaxLength(20);
            entity.Property(s => s.SidebarState).HasMaxLength(20);
            entity.Property(s => s.FontSize).HasMaxLength(20);
            entity.Property(s => s.CardStyle).HasMaxLength(20);
            entity.HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
