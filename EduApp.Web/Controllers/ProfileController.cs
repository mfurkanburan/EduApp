using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ProfileService _profileService;
    private readonly FileService _fileService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ProfileService profileService,
        FileService fileService,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _profileService = profileService;
        _fileService = fileService;
        _signInManager = signInManager;
    }

    [HttpGet("profil")]
    public async Task<IActionResult> Index()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        IList<string> roles = await _userManager.GetRolesAsync(user);

        int enrolledCourseCount = 0;
        int instructorCourseCount = 0;

        ProfileStatisticsResult statistics = new ProfileStatisticsResult();

        List<UserBadge> badges = new List<UserBadge>();

        if (roles.Contains(ApplicationRoles.Student))
        {
            statistics = await _profileService.GetStudentStatisticsAsync(user.Id);

            enrolledCourseCount = statistics.EnrolledCourseCount;

            badges = await _profileService.GetUserBadgesAsync(user.Id);
        }

        if (roles.Contains(ApplicationRoles.Instructor))
        {
            instructorCourseCount = await _profileService.GetInstructorCourseCountAsync(user.Id);
        }

        ProfileViewModel model = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            ProfileImagePath = user.ProfileImagePath,
            CreatedAt = user.CreatedAt,
            RoleName = GetRoleDisplayName(roles),
            EnrolledCourseCount = enrolledCourseCount,
            InstructorCourseCount = instructorCourseCount,
            CompletedCourseCount = statistics.CompletedCourseCount,
            TotalLessonCount = statistics.TotalLessonCount,
            CompletedLessonCount = statistics.CompletedLessonCount,
            OverallProgressPercentage = statistics.OverallProgressPercentage,
            Badges = badges.Where(userBadge => userBadge.Badge is not null)
                .Select(userBadge => new UserBadgeViewModel
                {
                    Name = userBadge.Badge!.Name,
                    Description = userBadge.Badge.Description,
                    Icon = userBadge.Badge.Icon,
                    EarnedAt = userBadge.EarnedAt
                })
                .ToList()
        };

        return View(model);
    }

    private static string GetRoleDisplayName(IEnumerable<string> roles)
    {
        if (roles.Contains(ApplicationRoles.Student))
        {
            return "Öğrenci";
        }

        if (roles.Contains(ApplicationRoles.Instructor))
        {
            return "Eğitmen";
        }

        if (roles.Contains(ApplicationRoles.Admin))
        {
            return "Yönetici";
        }

        return "Rol Bilinmiyor";
    }

    [HttpGet("profil/düzenle")]
    public async Task<IActionResult> Edit()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        EditProfileViewModel model = new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            ExistingProfileImagePath = user.ProfileImagePath
        };

        return View(model);
    }

    [HttpPost("profil/düzenle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        string? profileImageError = _fileService.ValidateImage(model.ProfileImage, false, "Profil fotoğrafı");

        if (profileImageError is not null)
        {
            ModelState.AddModelError(nameof(EditProfileViewModel.ProfileImage), profileImageError);
        }

        if (!ModelState.IsValid)
        {
            model.Email = user.Email ?? "";
            model.ExistingProfileImagePath = user.ProfileImagePath;

            return View(model);
        }

        string? oldProfileImagePath = user.ProfileImagePath;

        string? profileImagePath = oldProfileImagePath;

        string? newlySavedProfileImagePath = null;

        if (model.ProfileImage is not null)
        {
            newlySavedProfileImagePath = await _fileService.SaveFileAsync(model.ProfileImage, "profiles");

            profileImagePath = newlySavedProfileImagePath;
        }
        else if (model.RemoveExistingProfileImage)
        {
            profileImagePath = null;
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.ProfileImagePath = profileImagePath;

        IdentityResult result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            if (newlySavedProfileImagePath is not null) _fileService.DeleteFile(newlySavedProfileImagePath, "profiles");

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Email = user.Email ?? "";
            model.ExistingProfileImagePath = oldProfileImagePath;

            return View(model);
        }

        if (oldProfileImagePath != profileImagePath) _fileService.DeleteFile(oldProfileImagePath, "profiles");

        TempData["ProfileMessage"] = "Profiliniz başarıyla güncellendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("profil/parola")]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost("profil/parola")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        if (model.CurrentPassword == model.NewPassword)
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Yeni parola mevcut parolanızla aynı olamaz.");
            return View(model);
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ToTurkishMessage());
            }

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["ProfileMessage"] = "Parolanız başarıyla değiştirildi.";

        return RedirectToAction(nameof(Index));
    }
}