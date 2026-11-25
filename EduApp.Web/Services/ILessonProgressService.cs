using EduApp.Web.Services.Results;
using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ILessonProgressService
{
    Task<LessonProgressResult> CompleteLessonAsync(string studentId, int lessonId);
    Task<CourseProgressResult> GetCourseProgressAsync(string studentId, int courseId);
}