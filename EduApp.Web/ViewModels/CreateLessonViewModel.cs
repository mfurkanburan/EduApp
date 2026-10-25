using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class CreateLessonViewModel
{
    public int CourseSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string SectionTitle { get; set; } = "";

    [Required(ErrorMessage = "Ders başlığı boş bırakılamaz.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Ders başlığı 5 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Ders içeriği boş bırakılamaz.")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Ders içeriği 10 ile 5000 karakter arasında olmalıdır.")]
    public string Content { get; set; } = "";   

    public IFormFile? MaterialFile { get; set; }
}