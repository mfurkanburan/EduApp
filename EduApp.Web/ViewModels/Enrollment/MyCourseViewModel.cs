namespace EduApp.Web.ViewModels.Enrollment;

public class MyCourseViewModel
{
    public int CourseId { get; set; }
    
    public string Title { get; set; } = "";

    public string CategoryName { get; set; } = "";

    public string InstructorName { get; set; } = "";

    public string? CoverImagePath { get; set; }

    public DateTime EnrolledAt { get; set; }

    public int TotalLessonCount { get; set; }

    public int CompletedLessonCount { get; set; }

    public int ProgressPercentage { get; set; }

    public bool IsCompleted { get; set; }
}