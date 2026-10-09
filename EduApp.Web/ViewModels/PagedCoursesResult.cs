using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

public class PagedCoursesResult
{
    public int TotalPages { get; set; }

    public List<Course> Courses { get; set; } = new List<Course>();
}