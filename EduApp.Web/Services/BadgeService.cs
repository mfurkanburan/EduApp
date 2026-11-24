using EduApp.Web.Constants;
using EduApp.Web.Data;
using EduApp.Web.Models;
using EduApp.Web.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class BadgeService : IBadgeService
{
    private readonly ApplicationDbContext _context;
    private readonly IProfileService _profileService;

    public BadgeService(ApplicationDbContext context, IProfileService profileService)
    {
        _context = context;
        _profileService = profileService;
    }

    private async Task AwardBadgeAsync(string userId, int badgeId)
    {
        bool alreadyHasBadge = await _context.UserBadges.AnyAsync(userBadge => userBadge.UserId == userId && userBadge.BadgeId == badgeId);

        if (alreadyHasBadge) return;

        _context.UserBadges.Add(new UserBadge
        {
            UserId = userId,
            BadgeId = badgeId,
            EarnedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task CheckAndAssignBadgesAsync(string userId)
    {
        ProfileStatisticsResult stats = await _profileService.GetStudentStatisticsAsync(userId);

        if (stats.CompletedLessonCount >= 1)
        {
            await AwardBadgeAsync(userId, BadgeIds.FirstLesson);
        }

        if (stats.CompletedLessonCount >= 10)
        {
            await AwardBadgeAsync(userId, BadgeIds.TenLessons);
        }

        if (stats.CompletedLessonCount >= 1)
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
            .Include(userBadge => userBadge.Badge)
            .AsNoTracking()
            .Where(userBadge => userBadge.UserId == userId)
            .OrderByDescending(userBadge => userBadge.EarnedAt)
            .ToListAsync();
    }
}