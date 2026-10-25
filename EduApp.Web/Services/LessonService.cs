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

    public async Task<Lesson?> GetLessonByIdAsync(int lessonId)
    {
        return await _context.Lessons
            .Include(lesson => lesson.CourseSection)
            .ThenInclude(section => section!.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(lesson => lesson.Id == lessonId);
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

    public async Task UpdateLessonAsync(Lesson lesson)
    {
        Lesson? existingLesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lesson.Id);

        if (existingLesson is null) return;

        existingLesson.Title = lesson.Title;
        existingLesson.Content = lesson.Content;
        existingLesson.MaterialPath = lesson.MaterialPath;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteLessonAsync(int id)
    {
        Lesson? lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);

        if (lesson is null) return false;

        int sectionId = lesson.CourseSectionId;

        _context.Lessons.Remove(lesson);

        await _context.SaveChangesAsync();

        await ReorderLessonsAsync(sectionId);

        return true;
    }

    public async Task ReorderLessonsAsync(int sectionId)
    {
        List<Lesson> lessons = await _context.Lessons
            .Where(lesson => lesson.CourseSectionId == sectionId)
            .OrderBy(lesson => lesson.Order)
            .ThenBy(lesson => lesson.Id)
            .ToListAsync();

        for (int i = 0; i < lessons.Count; i++)
        {
            lessons[i].Order = i + 1;
        }

        await _context.SaveChangesAsync();
    }
}