namespace EduApp.Web.Services.Results;

public class ProfileStatisticsResult
{
    public int EnrolledCourseCount { get; set; }
    public int CompletedCourseCount { get; set; }
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int OverallProgressPercentage { get; set; }
}