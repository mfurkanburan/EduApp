using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface ILessonService
{
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task<int> GetNextOrderAsync(int sectionId);
    Task CreateLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task<bool> DeleteLessonAsync(int id);
    Task ReorderLessonsAsync(int sectionId);
}