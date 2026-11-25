using EduApp.Web.Data;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class EnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Enrollment (Kayıt)

    public async Task<bool> IsEnrolledAsync(string studentId, int courseId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<EnrollmentResult> EnrollmentResultAsync(string studentId, int courseId)
    {
        bool courseExists = await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Id == courseId);

        if (!courseExists) return EnrollmentResult.CourseNotFound;

        bool alreadyEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (alreadyEnrolled) return EnrollmentResult.AlreadyEnrolled;

        _context.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return EnrollmentResult.Enrolled;
    }

    public async Task<List<Enrollment>> GetStudentEnrollmentsAsync(string studentId)
    {
        return await _context.Enrollments
            .Include(enrollment => enrollment.Course)
            .ThenInclude(course => course!.Category)
            .Include(enrollment => enrollment.Course)
            .ThenInclude(course => course!.Instructor)
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId)
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .ToListAsync();
    }

    // Progress (Ders İlerlemesi)

    public async Task<LessonProgressResult> CompleteLessonAsync(string studentId, int lessonId)
    {
        Lesson? lesson = await _context.Lessons
            .Include(l => l.CourseSection)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson is null || lesson.CourseSection is null) return LessonProgressResult.LessonNotFound;

        int courseId = lesson.CourseSection.CourseId;

        bool isEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (!isEnrolled) return LessonProgressResult.NotEnrolled;

        bool alreadyCompleted = await _context.LessonProgresses
            .AsNoTracking()
            .AnyAsync(p => p.StudentId == studentId && p.LessonId == lessonId);

        if (alreadyCompleted) return LessonProgressResult.AlreadyCompleted;

        _context.LessonProgresses.Add(new LessonProgress
        {
            StudentId = studentId,
            LessonId = lessonId,
            CompletedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return LessonProgressResult.Completed;
    }

    public async Task<CourseProgressResult> GetCourseProgressAsync(string studentId, int courseId)
    {
        List<int> lessonIds = await _context.Lessons
            .AsNoTracking()
            .Where(l => l.CourseSection != null && l.CourseSection.CourseId == courseId)
            .Select(l => l.Id)
            .ToListAsync();

        if (lessonIds.Count == 0)
        {
            return new CourseProgressResult();
        }

        List<int> completedLessonIds = await _context.LessonProgresses
            .AsNoTracking()
            .Where(p => p.StudentId == studentId && lessonIds.Contains(p.LessonId))
            .Select(p => p.LessonId)
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

public class CourseProgressResult
{
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public HashSet<int> CompletedLessonIds { get; set; } = new HashSet<int>();
}