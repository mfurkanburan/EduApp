using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalCourseCount { get; set; }
    public int TotalCategoryCount { get; set; }
    public int TotalUserCount { get; set; }
    public int TotalEnrollmentCount { get; set; }
    public List<AdminCourseItemViewModel> Courses { get; set; } = new List<AdminCourseItemViewModel>();
}

public class AdminCourseItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public CourseLevel Level { get; set; }
    public int SectionCount { get; set; }
    public int LessonCount { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}