using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

public class CourseListViewModel
{
    public string? Search { get; set; }

    public List<Course> Courses { get; set; } = new List<Course>();
}