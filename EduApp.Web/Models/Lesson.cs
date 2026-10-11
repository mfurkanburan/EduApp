namespace EduApp.Web.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public int CourseSectionId { get; set; }
    public CourseSection? CourseSection { get; set; }
}