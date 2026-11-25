namespace EduApp.Web.Models;

public static class ApplicationRoles
{
    public const string Student = "Student";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";
    public const string InstructorOrAdmin = Instructor + "," + Admin;
}

public static class BadgeIds
{
    public const int FirstLesson = 1;
    public const int TenLessons = 2;
    public const int FirstCourse = 3;
    public const int FiveCourses = 4;
}

public enum CourseLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3
}

public static class CourseLevelExtensions
{
    public static string ToDisplayName(this CourseLevel level)
    {
        return level switch
        {
            CourseLevel.Beginner => "Başlangıç",
            CourseLevel.Intermediate => "Orta",
            CourseLevel.Advanced => "İleri",
            _ => "Bilinmiyor"
        };
    }
}

public enum RegistrationRole
{
    Student = 1,
    Instructor = 2
}

public enum CategoryDeleteResult
{
    Deleted = 1,
    NotFound = 2,
    HasCourses = 3
}

public enum EnrollmentResult
{
    Enrolled = 1,
    AlreadyEnrolled = 2,
    CourseNotFound = 3
}

public enum LessonProgressResult
{
    Completed = 1,
    AlreadyCompleted = 2,
    LessonNotFound = 3,
    NotEnrolled = 4
}