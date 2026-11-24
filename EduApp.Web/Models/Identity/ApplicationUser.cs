using Microsoft.AspNetCore.Identity;

namespace EduApp.Web.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public List<Course> InstructedCourses { get; set; } = new List<Course>();
    public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public List<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public List<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}