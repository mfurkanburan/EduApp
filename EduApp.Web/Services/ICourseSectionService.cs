using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ICourseSectionService
{
    Task<CourseSection?> GetCourseSectionByIdAsync(int id);
    Task<int> GetNextSectionOrderAsync(int courseId);
    Task CreateCourseSectionAsync(CourseSection courseSection);
    Task UpdateCourseSectionAsync(CourseSection courseSection);
    Task<bool> DeleteCourseSectionAsync(int id);
    Task ReorderCourseSectionsAsync(int courseId);
}