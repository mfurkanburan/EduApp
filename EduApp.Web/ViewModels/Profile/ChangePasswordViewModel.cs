using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels.Profile;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Lütfen mevcut şifrenizi girin.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Lütfen yeni şifrenizi girin.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifreniz en az 8 karakter uzunluğunda olmalıdır.")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Lütfen yeni şifrenizi tekrar girin.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Yeni şifre ve tekrar şifre eşleşmiyor.")]
    public string ConfirmNewPassword { get; set; } = "";
}