using System.ComponentModel.DataAnnotations;
using EduApp.Web.Enums;
using EduApp.Web.Models;

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

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçmelisiniz.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Kurs seviyesi gereklidir.")]
    public CourseLevel? Level { get; set; }

    public string? ExistingCoverImagePath { get; set; }

    public IFormFile? CoverImage { get; set; }

    public bool RemoveExistingCoverImage { get; set; }

    [Required(ErrorMessage = "En az bir kurs gereksinimi eklemelisiniz.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Kurs gereksinimleri 10 ile 1000 karakter arasında olmalıdır.")]
    public string RequirementsText { get; set; } = "";

    public List<Category> Categories { get; set; } = new List<Category>();
}