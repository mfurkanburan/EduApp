using EduApp.Web.Models.Identity;

namespace EduApp.Web.Models;

public class UserBadge
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }
    public int BadgeId { get; set; }
    public Badge? Badge { get; set; }
    public DateTime EarnedAt { get; set; }
}