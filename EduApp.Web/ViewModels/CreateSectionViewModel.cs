using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class CreateSectionViewModel
{
    public int CourseId { get; set; }
    
    public string CourseTitle { get; set; } = "";

    [Required(ErrorMessage = "Bölüm başlığı boş bırakılamaz.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Bölüm başlığı 3 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";
}