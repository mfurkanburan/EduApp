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

        // Fluent API
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
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<CourseRequirement> CourseRequirements => Set<CourseRequirement>();
}