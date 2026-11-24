using EduApp.Web.Services.Results;

namespace EduApp.Web.Services;

public interface IProfileService
{
    Task<int> GetEnrolledCourseCountAsync(string userId);
    Task<int> GetInstructorCourseCountAsync(string userId);
    Task<ProfileStatisticsResult> GetStudentStatisticsAsync(string studentId);
}