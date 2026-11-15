namespace EduApp.Web.ViewModels;

public class DeleteCategoryViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CourseCount { get; set; }
}