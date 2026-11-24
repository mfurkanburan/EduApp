namespace EduApp.Web.ViewModels.Profile;

public class ProfileViewModel
{
    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public string RoleName { get; set; } = "";

    public string? ProfileImagePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public int EnrolledCourseCount { get; set; }

    public int InstructorCourseCount { get; set; }
    
    public int CompletedCourseCount { get; set; }

    public int TotalLessonCount { get; set; }

    public int CompletedLessonCount { get; set; }

    public int OverallProgressPercentage { get; set; }

    public List<UserBadgeViewModel> Badges { get; set; } = new List<UserBadgeViewModel>();
}