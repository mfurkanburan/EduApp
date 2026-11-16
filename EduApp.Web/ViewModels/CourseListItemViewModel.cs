using EduApp.Web.Enums;

namespace EduApp.Web.ViewModels;

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public CourseLevel Level { get; set; }
    public string? CoverImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
}