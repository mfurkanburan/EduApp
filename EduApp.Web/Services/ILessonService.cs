using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ILessonService
{
    Task<int> GetNextOrderAsync(int sectionId);
    Task CreateLessonAsync(Lesson lesson);
}