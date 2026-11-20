namespace EduApp.Web.ViewModels.Enrollment;

public class MyCourseViewModel
{
    public int CourseId { get; set; }
    
    public string Title = "";
    
    public string CategoryName = "";

    public string InstructorName = "";

    public string? CoverImagePath { get; set; }

    public DateTime EnrolledAt { get; set; }
}