namespace EduApp.Web.ViewModels;

public class SectionViewModel
{
    public int Id { get; set; }
    
    public string Title { get; set; } = "";
    
    public int Order { get; set; }

    public bool ContainsSelectedLesson { get; set; }

    public List<LessonListItemViewModel> Lessons { get; set; } = new List<LessonListItemViewModel>();
}