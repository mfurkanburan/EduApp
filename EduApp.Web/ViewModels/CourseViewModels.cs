using System.ComponentModel.DataAnnotations;
using EduApp.Web.Models;

namespace EduApp.Web.ViewModels;

// Course List (Kurs Listesi)

public class PagedCoursesResult
{
    public int TotalPages { get; set; }
    public List<Course> Courses { get; set; } = new List<Course>();
}

public class CourseListViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public CourseLevel? Level { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<CourseListItemViewModel> Courses { get; set; } = new List<CourseListItemViewModel>();
}

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public CourseLevel Level { get; set; }
    public string? CoverImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InstructorName { get; set; } = "";
    public string? InstructorProfileImagePath { get; set; }
}

// Course Details (Kurs Detayları)

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
    public int TotalLessonCount { get; set; }
    public int CompletedLessonCount { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCourseCompleted { get; set; }
    public bool CanCompleteSelectedLesson { get; set; }
    public bool IsSelectedLessonCompleted { get; set; }
}

public class CourseRequirementViewModel
{
    public string Text { get; set; } = "";
    public int Order { get; set; }
}

public class SectionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public bool ContainsSelectedLesson { get; set; }
    public List<LessonListItemViewModel> Lessons { get; set; } = new List<LessonListItemViewModel>();
}

public class LessonListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int Order { get; set; }
    public bool IsSelected { get; set; }
    public bool IsCompleted { get; set; }
}

public class SelectedLessonViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? MaterialFilePath { get; set; }
    public int Order { get; set; }
    public string SectionTitle { get; set; } = "";
}

// Course (Kurs)

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "Kurs başlığı gereklidir.")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Kurs başlığı 10 ile 200 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Kurs açıklaması gereklidir.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Kurs açıklaması 10 ile 2000 karakter arasında olmalıdır.")]
    public string Description { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Kurs kategorisi seçilmelidir.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Kurs seviyesi seçilmelidir.")]
    public CourseLevel? Level { get; set; }

    public IFormFile? CoverImage { get; set; }

    [Required(ErrorMessage = "Kurs gereksinimleri gereklidir.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Kurs gereksinimleri 10 ile 1000 karakter arasında olmalıdır.")]
    public string RequirementsText { get; set; } = "";

    public List<Category> Categories { get; set; } = new List<Category>();
}

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

// Section (Bölüm)

public class CreateSectionViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";

    [Required(ErrorMessage = "Bölüm başlığı boş bırakılamaz.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Bölüm başlığı 3 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";
}

public class EditSectionViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";

    [Required(ErrorMessage = "Bölüm başlığı gereklidir.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Bölüm başlığı 3 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Bölüm sırası gereklidir.")]
    [Range(1, int.MaxValue, ErrorMessage = "Bölüm sırası 1 veya daha büyük olmalıdır.")]
    public int Order { get; set; }
}

public class DeleteSectionViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public int LessonCount { get; set; }
}

// Lesson (Ders)

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
    [StringLength(50000, ErrorMessage = "Ders içeriği izin verilen maksimum uzunluğu aşıyor.")]
    public string Content { get; set; } = "";

    public IFormFile? MaterialFile { get; set; }
}

public class EditLessonViewModel
{
    public int Id { get; set; }
    public int CourseSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string SectionTitle { get; set; } = "";

    [Required(ErrorMessage = "Ders başlığı boş bırakılamaz.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "Ders başlığı 5 ile 150 karakter arasında olmalıdır.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Ders içeriği boş bırakılamaz.")]
    [StringLength(50000, ErrorMessage = "Ders içeriği izin verilen maksimum uzunluğu aşıyor.")]
    public string Content { get; set; } = "";

    public string? ExistingMaterialPath { get; set; }
    public IFormFile? MaterialFile { get; set; }
    public bool RemoveExistingMaterial { get; set; }
}

public class DeleteLessonViewModel
{
    public int Id { get; set; }
    public int CourseSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public string SectionTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string? MaterialPath { get; set; }
}

// Instructor (Eğitmen)

public class InstructorCourseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public CourseLevel Level { get; set; }
    public string? CoverImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SectionCount { get; set; }
    public int LessonCount { get; set; }
    public int StudentCount { get; set; }
}

// Home (Ana Sayfa)

public class HomeViewModel
{
    public int TotalCourseCount { get; set; }
    public int TotalCategoryCount { get; set; }
    public int TotalLessonCount { get; set; }
    public List<CategoryListItemViewModel> Categories { get; set; } = new List<CategoryListItemViewModel>();
    public List<CourseListItemViewModel> LatestCourses { get; set; } = new List<CourseListItemViewModel>();
}