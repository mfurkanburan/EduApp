using EduApp.Web.Constants;
using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.Services.Results;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILessonProgressService _lessonProgressService;

    public CoursesController(
        ICourseService courseService,
        ICategoryService categoryService,
        IWebHostEnvironment environment,
        UserManager<ApplicationUser> userManager,
        IEnrollmentService enrollmentService,
        ILessonProgressService lessonProgressService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _environment = environment;
        _userManager = userManager;
        _enrollmentService = enrollmentService;
        _lessonProgressService = lessonProgressService;
    }

    private const long MaximumCoverImageSize = 5 * 1024 * 1024;
    private static readonly string[] AllowedCoverImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 9;

        if (page < 1) page = 1;

        PagedCoursesResult result = await _courseService.GetAllCoursesAsync(search, page, pageSize);

        if (result.TotalPages > 0 && page > result.TotalPages)
        {
            page = result.TotalPages;

            result = await _courseService.GetAllCoursesAsync(search, page, pageSize);
        }

        CourseListViewModel model = new CourseListViewModel
        {
            Search = search,
            CurrentPage = page,
            TotalPages = result.TotalPages,
            
            Courses = result.Courses.Select(c => new CourseListItemViewModel
            {
                Id = c.Id,
                Name = c.Title,
                Description = c.Description,
                CategoryName = c.Category?.Name ?? "Kategori bulunamadı",
                Level = c.Level,
                CoverImagePath = c.CoverImagePath,
                CreatedAt = c.CreatedAt,
                InstructorName = c.Instructor?.FullName ?? "Eğitmen atanmamış"
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = ApplicationRoles.Instructor)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        CreateCourseViewModel model = new CreateCourseViewModel
        {
            Categories = await _categoryService.GetAllCategoriesAsync()
        };

        return View(model);
    }

    [Authorize(Roles = ApplicationRoles.Instructor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model)
    {
        ValidateCoverImage(model.CoverImage);

        List<CourseRequirement> requirements = CreateRequirements(model.RequirementsText);

        if (requirements.Count == 0)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.RequirementsText), "En az bir gereksinim eklemelisiniz.");
        }

        List<Category> categories = await _categoryService.GetAllCategoriesAsync();

        bool categoryExists = categories.Any(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CategoryId), "Seçilen kategori bulunamadı. Lütfen geçerli bir kategori seçin.");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = categories;
            return View(model);
        }

        string coverImagePath = await SaveCoverImageAsync(model.CoverImage!);

        string? instructorId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(instructorId)) return Challenge();

        Course course = new Course
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CategoryId = model.CategoryId,
            Level = model.Level!.Value,
            CoverImagePath = coverImagePath,
            CreatedAt = DateTime.UtcNow,
            InstructorId = instructorId,
            Requirements = requirements
        };

        await _courseService.CreateCourseAsync(course);

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id, int? lessonId)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        string? currentUserId = _userManager.GetUserId(User);

        bool isStudent = User.IsInRole(ApplicationRoles.Student);

        bool isInstructor = User.IsInRole(ApplicationRoles.Instructor);

        bool isAdmin = User.IsInRole(ApplicationRoles.Admin);

        bool isCourseOwner =
            isInstructor &&
            !string.IsNullOrWhiteSpace(currentUserId) &&
            course.InstructorId == currentUserId;

        bool isEnrolled = false;

        CourseProgressResult courseProgress = new CourseProgressResult();

        if (isStudent && !string.IsNullOrWhiteSpace(currentUserId))
        {
            isEnrolled = await _enrollmentService.IsEnrolledAsync(currentUserId, course.Id);

            if (isEnrolled)
            {
                courseProgress = await _lessonProgressService.GetCourseProgressAsync(currentUserId, course.Id);
            }
        }

        bool canManageCourse = isAdmin || isCourseOwner;

        bool canAccessCourseContent = canManageCourse || isEnrolled;

        bool canEnroll = isStudent && !isEnrolled;

        List<Lesson> allLessons = course.Sections
            .OrderBy(s => s.Order)
            .SelectMany(s => s.Lessons.OrderBy(l => l.Order))
            .ToList();

        Lesson? selectedLesson = null;

        if (lessonId.HasValue)
        {
            selectedLesson = allLessons.FirstOrDefault(l => l.Id == lessonId.Value);

            if (selectedLesson is null) return NotFound();
        }

        selectedLesson ??= allLessons.FirstOrDefault();

        CourseSection? selectedSection = selectedLesson is null ? null : course.Sections.FirstOrDefault(s => s.Id == selectedLesson.CourseSectionId);

        List<SectionViewModel> sectionModels = course.Sections
            .OrderBy(s => s.Order)
            .Select(s => new SectionViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Order = s.Order,

                ContainsSelectedLesson = selectedSection is not null && s.Id == selectedSection.Id,

                Lessons = s.Lessons
                    .OrderBy(l => l.Order)
                    .Select(l => new LessonListItemViewModel
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Order = l.Order,
                        IsSelected = selectedLesson is not null && l.Id == selectedLesson.Id,
                        IsCompleted = courseProgress.CompletedLessonIds.Contains(l.Id)
                    }).ToList()
            }).ToList();

        bool selectedLessonCompleted = selectedLesson is not null && courseProgress.CompletedLessonIds.Contains(selectedLesson.Id);

        bool canCompleteSelectedLesson = isStudent && isEnrolled && selectedLesson is not null && !selectedLessonCompleted;

        SelectedLessonViewModel? selectedLessonModel = selectedLesson is null ? null : new SelectedLessonViewModel
        {
            Id = selectedLesson.Id,
            Title = selectedLesson.Title,
            Content = canAccessCourseContent ? selectedLesson.Content : "Bu dersin içeriğine erişim izniniz yok.",
            MaterialFilePath = canAccessCourseContent ? selectedLesson.MaterialPath : null,
            Order = selectedLesson.Order,
            SectionTitle = selectedSection?.Title ?? "Bölüm bulunamadı"
        };

        CourseDetailsViewModel model = new CourseDetailsViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName ?? "Eğitmen atanmamış",
            CanManageCourse = CanManageCourse(course),
            CreatedAt = course.CreatedAt,
            CategoryName = course.Category?.Name ?? "Kategori bulunamadı",
            Level = course.Level,
            CoverImagePath = course.CoverImagePath,
            Requirements = course.Requirements
                .OrderBy(r => r.Order)
                .Select(r => new CourseRequirementViewModel
                {
                    Text = r.Text,
                    Order = r.Order
                }).ToList(),
            Sections = sectionModels,
            SelectedLesson = selectedLessonModel,
            IsEnrolled = isEnrolled,
            CanAccessCourseContent = canAccessCourseContent,
            CanEnroll = canEnroll,
            TotalLessonCount = courseProgress.TotalLessonCount,
            CompletedLessonCount = courseProgress.CompletedLessonCount,
            ProgressPercentage = courseProgress.ProgressPercentage,
            IsCourseCompleted = courseProgress.IsCompleted,
            CanCompleteSelectedLesson = canCompleteSelectedLesson,
            IsSelectedLessonCompleted = selectedLessonCompleted
        };

        return View(model);
    }

    [Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        if (!CanManageCourse(course)) return Forbid();

        EditCourseViewModel model = new EditCourseViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            Level = course.Level,
            ExistingCoverImagePath = course.CoverImagePath,
            RequirementsText = 
                string.Join(
                    Environment.NewLine,
                    course.Requirements
                        .OrderBy(r => r.Order)
                        .Select(r => r.Text)),
            Categories = await _categoryService.GetAllCategoriesAsync()
        };

        return View(model);
    }

    [Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCourseViewModel model)
    {
        Course? existingCourse = await _courseService.GetCourseByIdAsync(model.Id);

        if (existingCourse is null) return NotFound();

        if (!CanManageCourse(existingCourse)) return Forbid();

        ValidateOptionalCoverImage(model.CoverImage);

        List<CourseRequirement> requirements = CreateRequirements(model.RequirementsText);

        if (requirements.Count == 0)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.RequirementsText), "En az bir gereksinim eklemelisiniz.");
        }

        List<Category> categories = await _categoryService.GetAllCategoriesAsync();

        bool categoryExists = categories.Any(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CategoryId), "Seçilen kategori bulunamadı. Lütfen geçerli bir kategori seçin.");
        }

        bool willHaveNewCoverImage = !string.IsNullOrWhiteSpace(model.ExistingCoverImagePath) && !model.RemoveExistingCoverImage;
    
        if (model.CoverImage is not null)
        {
            willHaveNewCoverImage = true;
        }

        if (!willHaveNewCoverImage)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CoverImage), "Kurs kapak görseli zorunludur. Lütfen yeni bir kapak görseli yükleyin veya mevcut kapak görselini kaldırmayın.");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = categories;
            model.ExistingCoverImagePath = existingCourse.CoverImagePath;
            return View(model);
        }

        string? coverImagePath = existingCourse.CoverImagePath;
        string? oldCoverImagePath = existingCourse.CoverImagePath;

        if (model.CoverImage is not null)
        {
            coverImagePath = await SaveCoverImageAsync(model.CoverImage);
        }
        else if (model.RemoveExistingCoverImage)
        {
            coverImagePath = null;
        }

        Course updatedCourse = new Course
        {
            Id = existingCourse.Id,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CategoryId = model.CategoryId,
            Level = model.Level!.Value,
            CoverImagePath = coverImagePath,
            Requirements = requirements
        };

        bool updated = await _courseService.UpdateCourseAsync(updatedCourse);

        if (!updated)
        {
            if (model.CoverImage is not null)
            {
                DeleteCoverImage(coverImagePath);
            }

            return NotFound();
        }

        if (oldCoverImagePath != coverImagePath)
        {
            DeleteCoverImage(oldCoverImagePath);
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        if (!CanManageCourse(course)) return Forbid();

        CourseDetailsViewModel model = new CourseDetailsViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description
        };

        return View(model);
    }

    [Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CourseDetailsViewModel model)
    {
        Course? course = await _courseService.GetCourseForDeleteAsync(model.Id);

        if (course is null) return NotFound();

        if (!CanManageCourse(course)) return Forbid();

        string? coverImagePath = course.CoverImagePath;

        List<string> materialPaths = course.Sections
            .SelectMany(s => s.Lessons)
            .Where(l => !string.IsNullOrWhiteSpace(l.MaterialPath))
            .Select(l => l.MaterialPath!)
            .ToList();

        bool deleted = await _courseService.DeleteCourseAsync(model.Id);

        if (!deleted) return NotFound();

        DeleteCoverImage(coverImagePath);

        foreach (string materialPath in materialPaths)
        {
            DeleteMaterialFileFromCourse(materialPath);
        }

        return RedirectToAction(nameof(Index));
    }

    private void DeleteMaterialFileFromCourse(string? materialPath)
    {
        if (string.IsNullOrWhiteSpace(materialPath)) return;

        string materialsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "materials");

        string fileName = Path.GetFileName(materialPath);

        if (string.IsNullOrWhiteSpace(fileName)) return;

        string physicalPath = Path.Combine(materialsDirectory, fileName);

        if (!physicalPath.StartsWith(materialsDirectory, StringComparison.OrdinalIgnoreCase)) return;

        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }
    }

    private void ValidateCoverImage(IFormFile? coverImage)
    {
        if (coverImage is null)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CoverImage), "Kurs kapak görseli zorunludur.");
            return;
        }

        if (coverImage.Length == 0)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CoverImage), "Kurs kapak görseli boş olamaz.");
            return;
        }

        if (coverImage.Length > MaximumCoverImageSize)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CoverImage), "Kurs kapak görseli 5 MB'den büyük olamaz.");
            return;
        }

        string extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedCoverImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CoverImage), "Kurs kapak görseli yalnızca .jpg, .jpeg, .png veya .webp formatında olmalıdır.");
        }
    }

    private async Task<string> SaveCoverImageAsync(IFormFile coverImage)
    {
        string extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();

        string coversDirectory = Path.Combine(_environment.WebRootPath, "uploads", "course-covers");

        Directory.CreateDirectory(coversDirectory);

        string generatedFileName = $"{Guid.NewGuid():N}{extension}";

        string physicalFilePath = Path.Combine(coversDirectory, generatedFileName);

        await using FileStream stream = new(physicalFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
    
        await coverImage.CopyToAsync(stream);

        return $"/uploads/course-covers/{generatedFileName}";
    }

    private static List<CourseRequirement> CreateRequirements(string requirementsText)
    {
        string[] requirementLines = requirementsText.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<CourseRequirement> requirements = new();

        for (int i = 0; i < requirementLines.Length; i++)
        {
            string requirementText = requirementLines[i];

            if (string.IsNullOrWhiteSpace(requirementText)) continue;

            requirements.Add(new CourseRequirement
            {
                Text = requirementText,
                Order = i + 1
            });
        }

        return requirements;
    }

    private void ValidateOptionalCoverImage(IFormFile? coverImage)
    {
        if (coverImage is null) return;

        if (coverImage.Length == 0)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CoverImage), "Kurs kapak görseli boş olamaz.");
            return;
        }

        if (coverImage.Length > MaximumCoverImageSize)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CoverImage), "Kurs kapak görseli 5 MB'den büyük olamaz.");
            return;
        }

        string extension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedCoverImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CoverImage), "Kurs kapak görseli yalnızca .jpg, .jpeg, .png veya .webp formatında olmalıdır.");
        }
    }

    private void DeleteCoverImage(string? coverImagePath)
    {
        if (string.IsNullOrWhiteSpace(coverImagePath)) return;

        string coversDirectory = Path.Combine(_environment.WebRootPath, "uploads", "course-covers");

        string fileName = Path.GetFileName(coverImagePath);

        if (string.IsNullOrWhiteSpace(fileName)) return;

        string physicalFilePath = Path.Combine(coversDirectory, fileName);

        if (!physicalFilePath.StartsWith(coversDirectory, StringComparison.OrdinalIgnoreCase)) return;

        if (System.IO.File.Exists(physicalFilePath))
        {
            System.IO.File.Delete(physicalFilePath);
        }
    }

    private bool CanManageCourse(Course course)
    {
        if (User.IsInRole(ApplicationRoles.Admin)) return true;

        if (!User.IsInRole(ApplicationRoles.Instructor)) return false;

        string? currentUserId = _userManager.GetUserId(User);

        return !string.IsNullOrWhiteSpace(currentUserId) && course.InstructorId == currentUserId;
    }
}