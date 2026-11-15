using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kategori adı 3 ile 100 karakter arasında olmalıdır.")]
    public string Name { get; set; } = "";
}