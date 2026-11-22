namespace EduApp.Web.Services.Results;

public class CourseProgressResult
{
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public HashSet<int> CompletedLessonIds { get; set; } = new HashSet<int>();
}