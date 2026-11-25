using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

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

public static class AppExtensions
{
    public static bool CanManageCourse(this ClaimsPrincipal user, Course course)
    {
        if (user.IsInRole(ApplicationRoles.Admin)) return true;
        if (!user.IsInRole(ApplicationRoles.Instructor)) return false;

        string? currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(currentUserId) && course.InstructorId == currentUserId;
    }

    public static string ToTurkishMessage(this IdentityError error)
    {
        return error.Code switch
        {
            "PasswordMismatch" => "Mevcut şifreniz yanlış.",
            "PasswordTooShort" => "Parola en az 8 karakter olmalıdır.",
            "PasswordRequiresDigit" => "Parola en az bir rakam içermelidir.",
            "PasswordRequiresLower" => "Parola en az bir küçük harf içermelidir.",
            "PasswordRequiresUpper" => "Parola en az bir büyük harf içermelidir.",
            "DuplicateEmail" => "Bu e-posta adresi zaten kayıtlı.",
            "DuplicateUserName" => "Bu e-posta adresi zaten kayıtlı.",
            "InvalidEmail" => "Geçerli bir e-posta adresi giriniz.",
            _ => error.Description
        };
    }
}