using EduApp.Web.Models;
using EduApp.Web.ViewModels;

namespace EduApp.Web.Services;

public interface ICourseService
{
    Task<PagedCoursesResult> GetAllCoursesAsync(string? search, int page, int pageSize);

    Task<Course?> GetCourseByIdAsync(int id);

    Task CreateCourseAsync(Course course);

    Task UpdateCourseAsync(Course course);

    Task DeleteCourseAsync(int id);
}