using EduApp.Web.Models;

namespace EduApp.Web.Services;

public interface IBadgeService
{
    Task CheckAndAssignBadgesAsync(string userId);
    Task<List<UserBadge>> GetUserBadgesAsync(string userId);
}