using System.ComponentModel.DataAnnotations;

namespace EduApp.Web.ViewModels;

public class CourseDetailsViewModel
{
    public int Id { get; set; }

    [Display(Name = "Kurs Başlığı")]
    public string Title { get; set; } = "";

    [Display(Name = "Kurs Açıklaması")]
    public string Description { get; set; } = "";

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; }
}