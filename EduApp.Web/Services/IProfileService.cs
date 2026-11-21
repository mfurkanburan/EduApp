namespace EduApp.Web.Services;

public interface IProfileService
{
    Task<int> GetEnrolledCourseCountAsync(string userId);
    Task<int> GetInstructorCourseCountAsync(string userId);
}