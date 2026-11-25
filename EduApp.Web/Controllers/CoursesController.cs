using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class CoursesController : Controller
{
    private readonly CourseService _courseService;
    private readonly CategoryService _categoryService;
    private readonly FileService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EnrollmentService _enrollmentService;

    public CoursesController(
        CourseService courseService,
        CategoryService categoryService,
        FileService fileService,
        UserManager<ApplicationUser> userManager,
        EnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _fileService = fileService;
        _userManager = userManager;
        _enrollmentService = enrollmentService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? categoryId, CourseLevel? level, int page = 1)
    {
        const int pageSize = 9;

        if (page < 1) page = 1;

        PagedCoursesResult result = await _courseService.GetAllCoursesAsync(search, categoryId, level, page, pageSize);

        if (result.TotalPages > 0 && page > result.TotalPages)
        {
            page = result.TotalPages;
            result = await _courseService.GetAllCoursesAsync(search, categoryId, level, page, pageSize);
        }

        CourseListViewModel model = new CourseListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Level = level,
            CurrentPage = page,
            TotalPages = result.TotalPages,
            Categories = await _categoryService.GetAllCategoriesAsync(),
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
        string? coverImageError = _fileService.ValidateImage(model.CoverImage, true, "Kurs kapak resmi");

        if (coverImageError is not null)
        {
            ModelState.AddModelError(nameof(CreateCourseViewModel.CoverImage), coverImageError);
        }

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

        string coverImagePath = await _fileService.SaveFileAsync(model.CoverImage!, "course-covers");

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
                courseProgress = await _enrollmentService.GetCourseProgressAsync(currentUserId, course.Id);
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
            CanManageCourse = User.CanManageCourse(course),
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

        if (!User.CanManageCourse(course)) return Forbid();

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

        if (!User.CanManageCourse(existingCourse)) return Forbid();

        string? coverImageError = _fileService.ValidateImage(model.CoverImage, false, "Kurs kapak resmi");

        if (coverImageError is not null)
        {
            ModelState.AddModelError(nameof(EditCourseViewModel.CoverImage), coverImageError);
        }

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
            await _fileService.SaveFileAsync(model.CoverImage, "course-covers");
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
                _fileService.DeleteFile(coverImagePath, "course-covers");
            }

            return NotFound();
        }

        if (oldCoverImagePath != coverImagePath)
        {
            _fileService.DeleteFile(oldCoverImagePath, "course-covers");
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        if (!User.CanManageCourse(course)) return Forbid();

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

        if (!User.CanManageCourse(course)) return Forbid();

        string? coverImagePath = course.CoverImagePath;

        List<string> materialPaths = course.Sections
            .SelectMany(s => s.Lessons)
            .Where(l => !string.IsNullOrWhiteSpace(l.MaterialPath))
            .Select(l => l.MaterialPath!)
            .ToList();

        bool deleted = await _courseService.DeleteCourseAsync(model.Id);

        if (!deleted) return NotFound();

        _fileService.DeleteFile(coverImagePath, "course-covers");

        foreach (string materialPath in materialPaths)
        {
            _fileService.DeleteFile(materialPath, "materials");
        }

        return RedirectToAction(nameof(Index));
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

    [Authorize(Roles = ApplicationRoles.Instructor)]
    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        string? instructorId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(instructorId)) return Challenge();

        List<InstructorCourseViewModel> courses = await _courseService.GetInstructorCoursesAsync(instructorId);
        return View(courses);
    }
}