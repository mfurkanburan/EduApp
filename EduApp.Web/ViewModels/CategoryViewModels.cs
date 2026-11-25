using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class CategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int CourseCount { get; set; }
}

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kategori adı 3 ile 100 karakter arasında olmalıdır.")]
    public string Name { get; set; } = "";
}

public class EditCategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kategori adı 3 ile 100 karakter arasında olmalıdır.")]
    public string Name { get; set; } = "";
}

public class DeleteCategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CourseCount { get; set; }
}