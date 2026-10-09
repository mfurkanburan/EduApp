using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ICourseService
{
    List<Course> GetAllCourses();

    Course? GetCourseById(int id);

    void CreateCourse(Course course);

    void UpdateCourse(Course course);

    void DeleteCourse(int id);
}