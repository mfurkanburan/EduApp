namespace EduApp.Web.Constants;

public static class ApplicationRoles
{
    public const string Student = "Student";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";
    public const string InstructorOrAdmin = Instructor + "," + Admin;
}