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
}