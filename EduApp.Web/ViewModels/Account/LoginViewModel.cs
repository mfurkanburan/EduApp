using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta alanı boş bırakılamaz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}