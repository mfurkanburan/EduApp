using System.ComponentModel.DataAnnotations;
using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "Kurs başlığı gereklidir.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Kurs başlığı 10 ile 200 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Kurs açıklaması gereklidir.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Kurs açıklaması 10 ile 2000 karakter arasında olmalıdır.")]
    public string Description { get; set; } = "";

    public int CategoryId { get; set; }

    public List<Category> Categories { get; set; } = new List<Category>();
}