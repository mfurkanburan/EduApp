using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class LessonsController : Controller
{
    private const long MaximumMaterialFileSize = 10 * 1024 * 1024;

    private static readonly string[] AllowedMaterialExtensions =
    {
        ".pdf",
        ".doc",
        ".docx",
        ".ppt",
        ".pptx",
        ".zip"
    };

    private readonly ICourseSectionService _sectionService;
    private readonly ILessonService _lessonService;
    private readonly IWebHostEnvironment _environment;

    public LessonsController(
        ICourseSectionService sectionService,
        ILessonService lessonService,
        IWebHostEnvironment environment)
    {
        _sectionService = sectionService;
        _lessonService = lessonService;
        _environment = environment;
    }

    [HttpGet("Lessons/Create/{sectionId:int}")]
    public async Task<IActionResult> Create(int sectionId)
    {
        CourseSection? section = await _sectionService.GetCourseSectionByIdAsync(sectionId);

        if (section is null) return NotFound();

        CreateLessonViewModel model = new()
        {
            CourseSectionId = section.Id,
            CourseId = section.CourseId,
            CourseTitle = section.Course?.Title ?? "",
            SectionTitle = section.Title
        };

        return View(model);
    }

    [HttpPost("Lessons/Create/{sectionId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int sectionId,
        CreateLessonViewModel model)
    {
        if (sectionId != model.CourseSectionId) return BadRequest();

        CourseSection? section = await _sectionService
                .GetCourseSectionByIdAsync(sectionId);

        if (section is null) return NotFound();

        if (section.CourseId != model.CourseId) return BadRequest();

        ValidateMaterialFile(model.MaterialFile);

        if (!ModelState.IsValid)
        {
            PrepareCreateViewModel(model, section);

            return View(model);
        }

        string? materialFilePath = await SaveMaterialFileAsync(model.MaterialFile);

        Lesson lesson = new()
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            MaterialPath = materialFilePath,
            Order = await _lessonService.GetNextOrderAsync(sectionId),
            CourseSectionId = sectionId
        };

        await _lessonService.CreateLessonAsync(lesson);

        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    private void ValidateMaterialFile(IFormFile? file)
    {
        if (file is null) return;

        if (file.Length == 0)
        {
            ModelState.AddModelError(
                nameof(CreateLessonViewModel.MaterialFile),
                "Seçilen dosya boş olamaz.");

            return;
        }

        if (file.Length > MaximumMaterialFileSize)
        {
            ModelState.AddModelError(nameof(CreateLessonViewModel.MaterialFile), "Dosya boyutu 10 MB'yi geçemez.");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedMaterialExtensions.Contains(extension))
        {
            ModelState.AddModelError(
                nameof(CreateLessonViewModel.MaterialFile),
                "Geçersiz dosya türü. Yalnızca PDF, DOC, DOCX, PPT, PPTX ve ZIP dosyaları yüklenebilir.");
        }
    }

    private async Task<string?> SaveMaterialFileAsync(
        IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        string uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "materials");

        Directory.CreateDirectory(uploadsDirectory);

        string generatedFileName = $"{Guid.NewGuid():N}{extension}";

        string physicalFilePath = Path.Combine(uploadsDirectory, generatedFileName);

        await using FileStream fileStream = new(
            physicalFilePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None
        );

        await file.CopyToAsync(fileStream);

        return $"/uploads/materials/{generatedFileName}";
    }

    private static void PrepareCreateViewModel(CreateLessonViewModel model, CourseSection section)
    {
        model.CourseSectionId = section.Id;
        model.CourseId = section.CourseId;
        model.CourseTitle = section.Course?.Title ?? "";
        model.SectionTitle = section.Title;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Lesson? lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson is null) return NotFound();

        EditLessonViewModel model = new()
        {
            Id = lesson.Id,
            CourseSectionId = lesson.CourseSectionId,
            CourseId = lesson.CourseSection?.CourseId ?? 0,
            CourseTitle = lesson.CourseSection?.Course?.Title ?? "",
            SectionTitle = lesson.CourseSection?.Title ?? "",
            Title = lesson.Title,
            Content = lesson.Content,
            ExistingMaterialPath = lesson.MaterialPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditLessonViewModel model)
    {
        Lesson? existingLesson = await _lessonService.GetLessonByIdAsync(model.Id);

        if (existingLesson is null) return NotFound();

        if (existingLesson.CourseSectionId != model.CourseSectionId) return BadRequest();

        int actualCourseId = existingLesson.CourseSection?.CourseId ?? 0;

        if (actualCourseId != model.CourseId) return BadRequest();

        ValidateMaterialFile(model.MaterialFile);

        if (!ModelState.IsValid)
        {
            PrepareEditViewModel(model, existingLesson);

            return View(model);
        }

        string? materialFilePath = existingLesson.MaterialPath;

        if (model.MaterialFile is not null)
        {
            string? newMaterialFilePath = await SaveMaterialFileAsync(model.MaterialFile);

            if (!ModelState.IsValid)
            {
                PrepareEditViewModel(model, existingLesson);

                return View(model);
            }

            DeleteMaterialFile(existingLesson.MaterialPath);
            materialFilePath = newMaterialFilePath;
        }
        else if (model.RemoveExistingMaterial)
        {
            DeleteMaterialFile(existingLesson.MaterialPath);
            materialFilePath = null;
        }

        Lesson updatedLesson = new()
        {
            Id = existingLesson.Id,
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            MaterialPath = materialFilePath,
        };

        await _lessonService.UpdateLessonAsync(updatedLesson);

        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    private static void PrepareEditViewModel(EditLessonViewModel model, Lesson existingLesson)
    {
        model.CourseSectionId = existingLesson.CourseSectionId;
        model.CourseId = existingLesson.CourseSection?.CourseId ?? 0;
        model.CourseTitle = existingLesson.CourseSection?.Course?.Title ?? "";
        model.SectionTitle = existingLesson.CourseSection?.Title ?? "";
        model.ExistingMaterialPath = existingLesson.MaterialPath;
    }

    private void DeleteMaterialFile(string? materialPath)
    {
        if (string.IsNullOrWhiteSpace(materialPath)) return;

        string materialsDirectory = Path.Combine(_environment.WebRootPath, "uploads", "materials");

        string fileName = Path.GetFileName(materialPath);

        if (string.IsNullOrWhiteSpace(fileName)) return;

        string pyhsicalFilePath = Path.Combine(materialsDirectory, fileName);

        if (!pyhsicalFilePath.StartsWith(materialsDirectory, StringComparison.OrdinalIgnoreCase)) return;

        if (System.IO.File.Exists(pyhsicalFilePath))
        {
            System.IO.File.Delete(pyhsicalFilePath);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Lesson? lesson = await _lessonService.GetLessonByIdAsync(id);

        if (lesson is null) return NotFound();

        DeleteLessonViewModel model = new()
        {
            Id = lesson.Id,
            CourseSectionId = lesson.CourseSectionId,
            CourseId = lesson.CourseSection?.CourseId ?? 0,
            CourseTitle = lesson.CourseSection?.Course?.Title ?? "",
            SectionTitle = lesson.CourseSection?.Title ?? "",
            Title = lesson.Title,
            MaterialPath = lesson.MaterialPath
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeleteLessonViewModel model)
    {
        Lesson? existingLesson = await _lessonService.GetLessonByIdAsync(model.Id);

        if (existingLesson is null) return NotFound();

        if (existingLesson.CourseSectionId != model.CourseSectionId) return BadRequest();

        int actualCourseId = existingLesson.CourseSection?.CourseId ?? 0;

        if (actualCourseId != model.CourseId) return BadRequest();

        string? materialPath = existingLesson.MaterialPath;

        bool deleted = await _lessonService.DeleteLessonAsync(existingLesson.Id);

        if (!deleted) return NotFound();

        DeleteMaterialFile(materialPath);

        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }
}