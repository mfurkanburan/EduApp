using EduApp.Web.Models;
using EduApp.Web.ViewModels;

namespace EduApp.Web.Services;

public interface ICourseService
{
    List<Course> GetAllCourses(string? search);

    Course? GetCourseById(int id);

    void CreateCourse(Course course);

    void UpdateCourse(Course course);

    void DeleteCourse(int id);

    PagedCoursesResult GetAllCoursesPaged(string? search, int page, int pageSize);
}