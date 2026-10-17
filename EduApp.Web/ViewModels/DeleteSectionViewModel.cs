namespace EduApp.Web.ViewModels;

public class DeleteSectionViewModel
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = "";

    public string Title { get; set; } = "";

    public int LessonCount { get; set; }
}