using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels.Profile;

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