using EduApp.Web.Data;
using EduApp.Web.Models;
using EduApp.Web.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class LessonProgressService : ILessonProgressService
{
    private readonly ApplicationDbContext _context;

    public LessonProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LessonProgressResult> CompleteLessonAsync(string studentId, int lessonId)
    {
        Lesson? lesson = await _context.Lessons
            .Include(lesson => lesson.CourseSection)
            .AsNoTracking()
            .FirstOrDefaultAsync(lesson => lesson.Id == lessonId);
        
        if (lesson is null || lesson.CourseSection is null) return LessonProgressResult.LessonNotFound;

        int courseId = lesson.CourseSection.CourseId;

        bool isEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(enrollment => enrollment.StudentId == studentId && enrollment.CourseId == courseId);

        if (!isEnrolled) return LessonProgressResult.NotEnrolled;

        bool alreadyCompleted = await _context.LessonProgresses
            .AsNoTracking()
            .AnyAsync(progress => progress.StudentId == studentId && progress.LessonId == lessonId);

        if (alreadyCompleted) return LessonProgressResult.AlreadyCompleted;

        LessonProgress progress = new LessonProgress
        {
            StudentId = studentId,
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow
        };

        _context.LessonProgresses.Add(progress);

        await _context.SaveChangesAsync();

        return LessonProgressResult.Completed;
    }

    public async Task<CourseProgressResult> GetCourseProgressAsync(string studentId, int courseId)
    {
        List<int> lessonIds = await _context.Lessons
            .AsNoTracking()
            .Where(lesson => lesson.CourseSection != null && lesson.CourseSection.CourseId == courseId)
            .Select(lesson => lesson.Id)
            .ToListAsync();
        
        if (lessonIds.Count == 0)
        {
            return new CourseProgressResult
            {
                TotalLessonCount = 0,
                CompletedLessonCount = 0,
                ProgressPercentage = 0,
                IsCompleted = false
            };
        }

        List<int> completedLessonIds = await _context.LessonProgresses
            .AsNoTracking()
            .Where(progress => progress.StudentId == studentId && lessonIds.Contains(progress.LessonId))
            .Select(progress => progress.LessonId)
            .ToListAsync();

        int completedCount = completedLessonIds.Count;

        int progressPercentage = (int)Math.Round(completedCount * 100.0 / lessonIds.Count);

        return new CourseProgressResult
        {
            TotalLessonCount = lessonIds.Count,
            CompletedLessonCount = completedCount,
            ProgressPercentage = progressPercentage,
            IsCompleted = completedCount == lessonIds.Count,
            CompletedLessonIds = completedLessonIds.ToHashSet()
        };
    }
}