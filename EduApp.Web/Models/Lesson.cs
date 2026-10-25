namespace EduApp.Web.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? MaterialPath { get; set; }
    public int Order { get; set; }
    public int CourseSectionId { get; set; }
    public CourseSection? CourseSection { get; set; }
}