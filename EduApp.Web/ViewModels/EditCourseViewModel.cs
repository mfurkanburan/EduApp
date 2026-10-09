using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class EditCourseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kurs başlığı gereklidir.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Kurs başlığı 10 ile 200 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Kurs açıklaması gereklidir.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Kurs açıklaması 10 ile 2000 karakter arasında olmalıdır.")]
    public string Description { get; set; } = "";
}