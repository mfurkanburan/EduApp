using System.ComponentModel.DataAnnotations;
using EduApp.Web.Enums;

namespace EduApp.Web.ViewModels;

public class CourseDetailsViewModel
{
    public int Id { get; set; }

    [Display(Name = "Kurs Başlığı")]
    public string Title { get; set; } = "";

    [Display(Name = "Kurs Açıklaması")]
    public string Description { get; set; } = "";

    public string? InstructorId { get; set; }

    public string InstructorName { get; set; } = "";

    public bool CanManageCourse { get; set; }

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Kategori")]
    public string CategoryName { get; set; } = "";

    [Display(Name = "Seviye")]
    public CourseLevel Level { get; set; }

    [Display(Name = "Kapak Resmi Yolu")]
    public string? CoverImagePath { get; set; }

    [Display(Name = "Gereksinimler")]
    public List<CourseRequirementViewModel> Requirements { get; set; } = new List<CourseRequirementViewModel>();

    public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();

    public SelectedLessonViewModel? SelectedLesson { get; set; }

    public bool IsEnrolled { get; set; }

    public bool CanAccessCourseContent { get; set; }

    public bool CanEnroll { get; set; }
}