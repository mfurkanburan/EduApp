namespace EduApp.Web.ViewModels;

public class SectionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public List<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}