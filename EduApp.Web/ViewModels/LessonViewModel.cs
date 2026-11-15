namespace EduApp.Web.ViewModels;

public class LessonViewModel
{
    public int Id { get; set; }
    
    public string Title { get; set; } = "";

    public string Content { get; set; } = "";

    public string? MaterialFilePath { get; set; }

    public int Order { get; set; }
}