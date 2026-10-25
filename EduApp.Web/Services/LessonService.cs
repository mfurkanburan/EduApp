using EduApp.Web.Data;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class LessonService : ILessonService
{
    private readonly ApplicationDbContext _context;

    public LessonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetNextOrderAsync(int sectionId)
    {
        int? maximumOrder = await _context.Lessons
            .Where(lesson => lesson.CourseSectionId == sectionId)
            .MaxAsync(lesson => (int?)lesson.Order);

        return (maximumOrder ?? 0) + 1;
    }

    public async Task CreateLessonAsync(Lesson lesson)
    {
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
    }
}