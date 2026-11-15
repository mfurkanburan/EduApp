namespace EduApp.Web.ViewModels;

public class DeleteLessonViewModel
{
    public int Id { get; set; }

    public int CourseSectionId { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = "";

    public string SectionTitle { get; set; } = "";

    public string Title { get; set; } = "";
    
    public string? MaterialPath { get; set; }
}