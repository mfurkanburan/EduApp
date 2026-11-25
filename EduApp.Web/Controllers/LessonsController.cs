using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
public class LessonsController : Controller
{
    private readonly CurriculumService _curriculumService;
    private readonly FileService _fileService;

    public LessonsController(
        CurriculumService curriculumService,
        FileService fileService)
    {
        _curriculumService = curriculumService;
        _fileService = fileService;
    }

    [HttpGet("Lessons/Create/{sectionId:int}")]
    public async Task<IActionResult> Create(int sectionId)
    {
        CourseSection? section = await _curriculumService.GetCourseSectionByIdAsync(sectionId);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();

        if (!User.CanManageCourse(section.Course)) return Forbid();

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

        CourseSection? section = await _curriculumService
                .GetCourseSectionByIdAsync(sectionId);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();

        if (!User.CanManageCourse(section.Course)) return Forbid();

        if (section.CourseId != model.CourseId) return BadRequest();

        string? materialFileError = _fileService.ValidateMaterial(model.MaterialFile);

        if (materialFileError is not null)
        {
            ModelState.AddModelError(nameof(CreateLessonViewModel.MaterialFile), materialFileError);
        }

        if (!ModelState.IsValid)
        {
            PrepareCreateViewModel(model, section);

            return View(model);
        }

        string? materialFilePath = model.MaterialFile is not null
            ? await _fileService.SaveFileAsync(model.MaterialFile, "materials")
            : null;

        Lesson lesson = new()
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            MaterialPath = materialFilePath,
            Order = await _curriculumService.GetNextOrderAsync(sectionId),
            CourseSectionId = sectionId
        };

        await _curriculumService.CreateLessonAsync(lesson);

        return RedirectToAction("Details", "Courses", new { id = model.CourseId, lessonId = lesson.Id });
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
        Lesson? lesson = await _curriculumService.GetLessonByIdAsync(id);

        if (lesson is null) return NotFound();

        Course? course = lesson.CourseSection?.Course;

        if (course is null) return NotFound();

        if (!User.CanManageCourse(course)) return Forbid();

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
        Lesson? existingLesson = await _curriculumService.GetLessonByIdAsync(model.Id);

        if (existingLesson is null) return NotFound();

        Course? course = existingLesson.CourseSection?.Course;

        if (course is null) return NotFound();

        if (!User.CanManageCourse(course)) return Forbid();

        if (existingLesson.CourseSectionId != model.CourseSectionId) return BadRequest();

        if (course.Id != model.CourseId) return BadRequest();

        string? materialFileError = _fileService.ValidateMaterial(model.MaterialFile);

        if (materialFileError is not null)
        {
            ModelState.AddModelError(nameof(EditLessonViewModel.MaterialFile), materialFileError);
        }

        if (!ModelState.IsValid)
        {
            PrepareEditViewModel(model, existingLesson);

            return View(model);
        }

        string? materialFilePath = existingLesson.MaterialPath;
        
        string? newMaterialFilePath = null;

        if (model.MaterialFile is not null)
        {
            newMaterialFilePath = await _fileService.SaveFileAsync(model.MaterialFile, "materials");

            materialFilePath = newMaterialFilePath;
        }
        else if (model.RemoveExistingMaterial)
        {
            materialFilePath = null;
        }

        Lesson updatedLesson = new()
        {
            Id = existingLesson.Id,
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            MaterialPath = materialFilePath
        };

        await _curriculumService.UpdateLessonAsync(updatedLesson);

        if (model.MaterialFile is not null && existingLesson.MaterialPath != newMaterialFilePath)
        {
            _fileService.DeleteFile(existingLesson.MaterialPath, "materials");
        }
        else if (model.RemoveExistingMaterial)
        {
            _fileService.DeleteFile(existingLesson.MaterialPath, "materials");
        }

        return RedirectToAction("Details", "Courses", new { id = model.CourseId, lessonId = existingLesson.Id });
    }

    private static void PrepareEditViewModel(EditLessonViewModel model, Lesson existingLesson)
    {
        model.CourseSectionId = existingLesson.CourseSectionId;
        model.CourseId = existingLesson.CourseSection?.CourseId ?? 0;
        model.CourseTitle = existingLesson.CourseSection?.Course?.Title ?? "";
        model.SectionTitle = existingLesson.CourseSection?.Title ?? "";
        model.ExistingMaterialPath = existingLesson.MaterialPath;
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Lesson? lesson = await _curriculumService.GetLessonByIdAsync(id);

        if (lesson is null) return NotFound();

        Course? course = lesson.CourseSection?.Course;

        if (course is null) return NotFound();

        if (!User.CanManageCourse(course)) return Forbid();

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
        Lesson? lesson = await _curriculumService.GetLessonByIdAsync(model.Id);

        if (lesson is null) return NotFound();

        Course? course = lesson.CourseSection?.Course;

        if (course is null) return NotFound();

        if (!User.CanManageCourse(course)) return Forbid();

        if (lesson.CourseSectionId != model.CourseSectionId) return BadRequest();

        if (course.Id != model.CourseId) return BadRequest();

        string? materialPath = lesson.MaterialPath;

        bool deleted = await _curriculumService.DeleteLessonAsync(lesson.Id);

        if (!deleted) return NotFound();

        _fileService.DeleteFile(materialPath, "materials");

        return RedirectToAction("Details", "Courses", new { id = course.Id });
    }
}