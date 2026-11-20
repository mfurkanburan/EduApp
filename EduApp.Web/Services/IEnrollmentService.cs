using EduApp.Web.Enums;
using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface IEnrollmentService
{
    Task<bool> IsEnrolledAsync(string studentId, int courseId);

    Task<EnrollmentResult> EnrollmentResultAsync(string studentId, int courseId);

    Task<List<Enrollment>> GetStudentEnrollmentsAsync(string studentId);
}