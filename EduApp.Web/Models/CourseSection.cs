namespace EduApp.Web.Models;

public class CourseSection
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public List<Lesson> Lessons { get; set; } = new List<Lesson>();
}