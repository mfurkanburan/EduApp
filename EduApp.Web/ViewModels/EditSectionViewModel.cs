using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class EditSectionViewModel
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = "";

    [Required(ErrorMessage = "Bölüm başlığı gereklidir.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Bölüm başlığı 3 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Bölüm sırası 1 veya daha büyük olmalıdır.")]
    public int Order { get; set; }
}