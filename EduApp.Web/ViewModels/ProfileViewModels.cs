using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class ProfileViewModel
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string? ProfileImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EnrolledCourseCount { get; set; }
    public int InstructorCourseCount { get; set; }
    public int CompletedCourseCount { get; set; }
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int OverallProgressPercentage { get; set; }
    public List<UserBadgeViewModel> Badges { get; set; } = new List<UserBadgeViewModel>();
}

public class UserBadgeViewModel
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public DateTime EarnedAt { get; set; }
}

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2 ile 100 karakter arasında olmalıdır.")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad 2 ile 100 karakter arasında olmalıdır.")]
    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";
    public string? ExistingProfileImagePath { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public bool RemoveExistingProfileImage { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Lütfen mevcut parolanızı girin.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Lütfen yeni parolanızı girin.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parolanız en az 8 karakter uzunluğunda olmalıdır.")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Lütfen yeni parolanızı tekrar girin.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Yeni parola ve parola tekrarı eşleşmiyor.")]
    public string ConfirmNewPassword { get; set; } = "";
}