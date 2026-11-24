using EduApp.Web.Data;
using EduApp.Web.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILessonProgressService _lessonProgressService;

    public ProfileService(ApplicationDbContext context, ILessonProgressService lessonProgressService)
    {
        _context = context;
        _lessonProgressService = lessonProgressService;
    }

    public async Task<int> GetEnrolledCourseCountAsync(string userId)
    {
        return await _context.Enrollments.AsNoTracking().CountAsync(enrollment => enrollment.StudentId == userId);
    }

    public async Task<int> GetInstructorCourseCountAsync(string userId)
    {
        return await _context.Courses.AsNoTracking().CountAsync(course => course.InstructorId == userId);
    }

    public async Task<ProfileStatisticsResult> GetStudentStatisticsAsync(string studentId)
    {
        List<int> enrolledCourseIds = await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId)
            .Select(enrollment => enrollment.CourseId)
            .ToListAsync();
        
        int completedLessonCount = await _context.LessonProgresses
            .AsNoTracking()
            .CountAsync(progress => progress.StudentId == studentId);

        int totalLessonCount = await _context.Lessons
            .AsNoTracking()
            .CountAsync(lesson => lesson.CourseSection != null && enrolledCourseIds.Contains(lesson.CourseSection.CourseId));

        int completedCourseCount = 0;

        foreach (int courseId in enrolledCourseIds)
        {
            CourseProgressResult progress = await _lessonProgressService.GetCourseProgressAsync(studentId, courseId);

            if (progress.IsCompleted) completedCourseCount++;  
        }

        int overallProgress = 0;

        if (totalLessonCount > 0)
        {
            overallProgress = (int)Math.Round(completedLessonCount * 100.0 / totalLessonCount);
        }

        return new ProfileStatisticsResult
        {
            EnrolledCourseCount = enrolledCourseIds.Count,
            CompletedCourseCount = completedCourseCount,
            TotalLessonCount = totalLessonCount,
            CompletedLessonCount = completedLessonCount,
            OverallProgressPercentage = overallProgress
        };
    }
}