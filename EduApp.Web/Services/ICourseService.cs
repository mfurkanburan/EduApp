using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ICourseService
{
    List<Course> GetAllCourses();
    void CreateCourse(Course course);

    Course? GetCourseById(int id);
}