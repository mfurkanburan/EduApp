namespace EduApp.Web.Models;

public class CourseRequirement
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public int Order { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}