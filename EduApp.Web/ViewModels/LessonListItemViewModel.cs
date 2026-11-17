namespace EduApp.Web.ViewModels;

public class LessonListItemViewModel
{
    public int Id { get; set; }
    
    public string Title { get; set; } = "";

    public int Order { get; set; }

    public bool IsSelected { get; set; }
}