using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ICourseSectionService
{
    Task CreateCourseSectionAsync(CourseSection courseSection);
    Task<int> GetNextSectionOrderAsync(int courseId);
}