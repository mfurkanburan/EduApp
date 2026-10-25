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

    [HttpGet]
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

    [HttpPost]
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
}