using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

public class CourseListViewModel
{
    public string? Search { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public List<CourseListItemViewModel> Courses { get; set; } = new List<CourseListItemViewModel>();
}