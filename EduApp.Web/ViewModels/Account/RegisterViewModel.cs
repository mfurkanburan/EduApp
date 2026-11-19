using System.ComponentModel.DataAnnotations;
using EduApp.Web.Enums;

namespace EduApp.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad alanı 2 ile 100 karakter arasında olmalıdır.")]
    public string FirstName { get; set; } = "";
    
    [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad alanı 2 ile 100 karakter arasında olmalıdır.")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "E-posta alanı boş bırakılamaz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(300, ErrorMessage = "E-posta alanı en fazla 300 karakter olabilir.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre alanı en az 8 karakter olmalıdır.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Şifre tekrar alanı boş bırakılamaz.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Hesap türü alanı boş bırakılamaz.")]
    public RegistrationRole? Role { get; set; }
}