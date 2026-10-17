using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Data;

public class ApplicationDbContext : DbContext
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
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
}