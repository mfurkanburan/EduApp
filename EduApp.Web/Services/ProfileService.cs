using EduApp.Web.Data;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class ProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly EnrollmentService _enrollmentService;

    public ProfileService(ApplicationDbContext context, EnrollmentService enrollmentService)
    {
        _context = context;
        _enrollmentService = enrollmentService;
    }

    // Statistic (İstatistik)

    public async Task<int> GetEnrolledCourseCountAsync(string userId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.StudentId == userId);
    }

    public async Task<int> GetInstructorCourseCountAsync(string userId)
    {
        return await _context.Courses
            .AsNoTracking()
            .CountAsync(c => c.InstructorId == userId);
    }

    public async Task<ProfileStatisticsResult> GetStudentStatisticsAsync(string studentId)
    {
        List<int> enrolledCourseIds = await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseId)
            .ToListAsync();

        int completedLessonCount = await _context.LessonProgresses
            .AsNoTracking()
            .CountAsync(p => p.StudentId == studentId);

        int totalLessonCount = await _context.Lessons
            .AsNoTracking()
            .CountAsync(l => l.CourseSection != null && enrolledCourseIds.Contains(l.CourseSection.CourseId));

        int completedCourseCount = 0;

        foreach (int courseId in enrolledCourseIds)
        {
            CourseProgressResult progress = await _enrollmentService.GetCourseProgressAsync(studentId, courseId);
            if (progress.IsCompleted) completedCourseCount++;
        }

        int overallProgress = totalLessonCount > 0
            ? (int)Math.Round(completedLessonCount * 100.0 / totalLessonCount)
            : 0;

        return new ProfileStatisticsResult
        {
            EnrolledCourseCount = enrolledCourseIds.Count,
            CompletedCourseCount = completedCourseCount,
            TotalLessonCount = totalLessonCount,
            CompletedLessonCount = completedLessonCount,
            OverallProgressPercentage = overallProgress
        };
    }

    // Badge (Rozet)

    public async Task CheckAndAssignBadgesAsync(string userId)
    {
        ProfileStatisticsResult stats = await GetStudentStatisticsAsync(userId);

        if (stats.CompletedLessonCount >= 1)
        {
            await AwardBadgeAsync(userId, BadgeIds.FirstLesson);
        }

        if (stats.CompletedLessonCount >= 10)
        {
            await AwardBadgeAsync(userId, BadgeIds.TenLessons);
        }

        if (stats.CompletedCourseCount >= 1)
        {
            await AwardBadgeAsync(userId, BadgeIds.FirstCourse);
        }

        if (stats.CompletedCourseCount >= 5)
        {
            await AwardBadgeAsync(userId, BadgeIds.FiveCourses);
        }
    }

    public async Task<List<UserBadge>> GetUserBadgesAsync(string userId)
    {
        return await _context.UserBadges
            .Include(ub => ub.Badge)
            .AsNoTracking()
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.EarnedAt)
            .ToListAsync();
    }

    private async Task AwardBadgeAsync(string userId, int badgeId)
    {
        bool alreadyHasBadge = await _context.UserBadges
            .AnyAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);

        if (alreadyHasBadge) return;

        _context.UserBadges.Add(new UserBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            EarnedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}

public class ProfileStatisticsResult
{
    public int EnrolledCourseCount { get; set; }
    public int CompletedCourseCount { get; set; }
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int OverallProgressPercentage { get; set; }
}