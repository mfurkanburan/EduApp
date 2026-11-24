using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .HasMany(course => course.Sections)
            .WithOne(section => section.Course)
            .HasForeignKey(section => section.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseSection>()
            .HasMany(section => section.Lessons)
            .WithOne(lesson => lesson.CourseSection)
            .HasForeignKey(lesson => lesson.CourseSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseSection>()
            .HasIndex(section => new { section.CourseId, section.Order })
            .IsUnique();

        modelBuilder.Entity<Lesson>()
            .HasIndex(lesson => new { lesson.CourseSectionId, lesson.Order })
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasMany(course => course.Requirements)
            .WithOne(requirement => requirement.Course)
            .HasForeignKey(requirement => requirement.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseRequirement>()
            .HasIndex(requirement => new { requirement.CourseId, requirement.Order })
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasOne(course => course.Instructor)
            .WithMany(user => user.InstructedCourses)
            .HasForeignKey(course => course.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(course => course.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(course => course.Description)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(course => course.CoverImagePath)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<CourseRequirement>(entity =>
        {
            entity.Property(requirement => requirement.Text)
                .IsRequired()
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(category => category.Name)
                .IsUnique();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(user => user.ProfileImagePath)
                .HasMaxLength(500);

            entity.Ignore(user => user.FullName);  
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasOne(enrollment => enrollment.Student)
                .WithMany(user => user.Enrollments)
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(enrollment => new { enrollment.StudentId, enrollment.CourseId }).IsUnique();
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasOne(progress => progress.Student)
                .WithMany(user => user.LessonProgresses)
                .HasForeignKey(progress => progress.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(progress => progress.Lesson)
                .WithMany(lesson => lesson.Progress)
                .HasForeignKey(progress => progress.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(progress => new { progress.StudentId, progress.LessonId }).IsUnique();
        });

        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasOne(userBadge => userBadge.User)
                .WithMany(user => user.UserBadges)
                .HasForeignKey(userBadge => userBadge.UserId);

            entity.HasOne(userBadge => userBadge.Badge)
                .WithMany(badge => badge.UserBadges)
                .HasForeignKey(userBadge => userBadge.BadgeId);

            entity.HasIndex(userBadge => new { userBadge.UserId, userBadge.BadgeId }).IsUnique();
        });

        // Badge Seed
        modelBuilder.Entity<Badge>().HasData(
            new Badge { Id = 1, Name = "İlk Ders", Description = "İlk dersini tamamladın.", Icon = "📕" },
            new Badge { Id = 2, Name = "10 Ders", Description = "10 ders tamamladın.", Icon = "🚀" },
            new Badge { Id = 3, Name = "İlk Kurs", Description = "İlk kursunu tamamladın.", Icon = "🎓" },
            new Badge { Id = 4, Name = "5 Kurs", Description = "5 kurs tamamladın.", Icon = "🪎" }
        );
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseRequirement> CourseRequirements => Set<CourseRequirement>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
}