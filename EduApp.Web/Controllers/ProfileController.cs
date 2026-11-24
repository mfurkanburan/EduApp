using EduApp.Web.Constants;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.Services.Results;
using EduApp.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfileService _profileService;
    private readonly IWebHostEnvironment _environment;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(UserManager<ApplicationUser> userManager, IProfileService profileService, IWebHostEnvironment environment, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _profileService = profileService;
        _environment = environment;
        _signInManager = signInManager;
    }

    private const long MaximumProfileImageSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedProfileImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        IList<string> roles = await _userManager.GetRolesAsync(user);

        int enrolledCourseCount = 0;
        int instructorCourseCount = 0;

        ProfileStatisticsResult statistics = new ProfileStatisticsResult();

        if (roles.Contains(ApplicationRoles.Student))
        {
            statistics = await _profileService.GetStudentStatisticsAsync(user.Id);

            enrolledCourseCount = statistics.EnrolledCourseCount;
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
            OverallProgressPercentage = statistics.OverallProgressPercentage
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

    [HttpGet]
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

    private void ValidateProfileImage(IFormFile? profileImage)
    {
        if (profileImage is null) return;

        if (profileImage.Length == 0)
        {
            ModelState.AddModelError(nameof(EditProfileViewModel.ProfileImage), "Profil resmi boş olamaz.");
            return;
        }

        if (profileImage.Length > MaximumProfileImageSize)
        {
            ModelState.AddModelError(nameof(EditProfileViewModel.ProfileImage), "Profil resmi 5 MB'den büyük olamaz.");
            return;
        }

        string extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedProfileImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(EditProfileViewModel.ProfileImage), "Profil resmi yalnızca .jpg, .jpeg, .png veya .webp formatında olmalıdır.");
        }
    }

    private async Task<string> SaveProfileImageAsync(IFormFile profileImage)
    {
        string extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

        string profilesDirectory = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

        Directory.CreateDirectory(profilesDirectory);

        string generatedFileName = $"{Guid.NewGuid()}{extension}";

        string physicalFilePath = Path.Combine(profilesDirectory, generatedFileName);

        await using FileStream stream = new(physicalFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

        await profileImage.CopyToAsync(stream);

        return $"/uploads/profiles/{generatedFileName}";
    }

    private void DeleteProfileImage(string profileImagePath)
    {
        if (string.IsNullOrEmpty(profileImagePath)) return;

        string profilesDirectory = Path.Combine(_environment.WebRootPath, "uploads", "profiles");

        string fileName = Path.GetFileName(profileImagePath);

        if (string.IsNullOrEmpty(fileName)) return;

        string physicalFilePath = Path.GetFullPath(Path.Combine(profilesDirectory, fileName));

        if (!physicalFilePath.StartsWith(profilesDirectory, StringComparison.OrdinalIgnoreCase)) return;

        if (System.IO.File.Exists(physicalFilePath)) System.IO.File.Delete(physicalFilePath);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        ValidateProfileImage(model.ProfileImage);

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
            newlySavedProfileImagePath = await SaveProfileImageAsync(model.ProfileImage);

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
            if (newlySavedProfileImagePath is not null) DeleteProfileImage(newlySavedProfileImagePath);

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Email = user.Email ?? "";
            model.ExistingProfileImagePath = oldProfileImagePath;

            return View(model);
        }

        if (oldProfileImagePath != profileImagePath) DeleteProfileImage(oldProfileImagePath!);

        TempData["ProfileMessage"] = "Profiliniz başarıyla güncellendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    private static string TranslateIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordMismatch" => "Mevcut şifreniz yanlış.",
            "PasswordTooShort" => "Şifreniz çok kısa. En az 8 karakter uzunluğunda olmalıdır.",
            "PasswordRequiresDigit" => "Şifreniz en az bir rakam içermelidir.",
            "PasswordRequiresLower" => "Şifreniz en az bir küçük harf içermelidir.",
            "PasswordRequiresUpper" => "Şifreniz en az bir büyük harf içermelidir.",
            _ => error.Description
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApplicationUser? user = await _userManager.GetUserAsync(User);

        if (user is null) return Challenge();

        if (model.CurrentPassword == model.NewPassword)
        {
            ModelState.AddModelError(nameof(model.NewPassword), "Yeni şifre mevcut şifrenizle aynı olamaz.");
            return View(model);
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
            }

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["ProfileMessage"] = "Şifreniz başarıyla değiştirildi.";

        return RedirectToAction(nameof(Index));
    }
}