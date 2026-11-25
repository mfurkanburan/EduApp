using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.InstructorOrAdmin)]
public class CourseSectionsController : Controller
{
    private readonly CourseService _courseService;
    private readonly CurriculumService _curriculumService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseSectionsController(CourseService courseService, CurriculumService curriculumService, UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _curriculumService = curriculumService;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("Courses/{courseId:int}/Sections/Create")]
    public async Task<IActionResult> Create(int courseId)
    {
        Course? course = await _courseService.GetCourseByIdAsync(courseId);

        if (course is null) return NotFound();

        if (!CanManageCourse(course)) return Forbid();

        CreateSectionViewModel model = new CreateSectionViewModel
        {
            CourseId = courseId,
            CourseTitle = course.Title
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Courses/{courseId:int}/Sections/Create")]
    public async Task<IActionResult> Create(int courseId, CreateSectionViewModel model)
    {
        if (courseId != model.CourseId) return BadRequest();

        Course? course = await _courseService.GetCourseByIdAsync(courseId);

        if (course is null) return NotFound();

        if (!CanManageCourse(course)) return Forbid();

        if (!ModelState.IsValid)
        {
            model.CourseTitle = course.Title;

            return View(model);
        }

        int nextOrder = await _curriculumService.GetNextSectionOrderAsync(courseId);

        CourseSection newSection = new CourseSection
        {
            Title = model.Title.Trim(),
            Order = nextOrder,
            CourseId = courseId
        };

        await _curriculumService.CreateCourseSectionAsync(newSection);

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        CourseSection? section = await _curriculumService.GetCourseSectionByIdAsync(id);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();

        if (!CanManageCourse(section.Course)) return Forbid();

        EditSectionViewModel model = new EditSectionViewModel
        {
            Id = section.Id,
            CourseId = section.CourseId,
            CourseTitle = section.Course?.Title ?? "",
            Title = section.Title,
            Order = section.Order
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSectionViewModel model)
    {
        CourseSection? section = await _curriculumService.GetCourseSectionByIdAsync(model.Id);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();
        
        if (!CanManageCourse(section.Course)) return Forbid();

        if (section.CourseId != model.CourseId) return BadRequest();

        if (!ModelState.IsValid)
        {
            model.CourseTitle = section.Course?.Title ?? "";

            return View(model);
        }

        CourseSection updatedSection = new CourseSection
        {
            Id = model.Id,
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            Order = model.Order
        };

        await _curriculumService.UpdateCourseSectionAsync(updatedSection);
        await _curriculumService.ReorderCourseSectionsAsync(model.CourseId);
        
        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        CourseSection? section = await _curriculumService.GetCourseSectionByIdAsync(id);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();

        if (!CanManageCourse(section.Course)) return Forbid();

        DeleteSectionViewModel model = new DeleteSectionViewModel
        {
            Id = section.Id,
            CourseId = section.CourseId,
            CourseTitle = section.Course?.Title ?? "",
            Title = section.Title,
            LessonCount = section.Lessons.Count
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeleteSectionViewModel model)
    {
        CourseSection? section = await _curriculumService.GetCourseSectionByIdAsync(model.Id);

        if (section is null) return NotFound();

        if (section.Course is null) return NotFound();

        if (!CanManageCourse(section.Course)) return Forbid();

        if (section.CourseId != model.CourseId) return BadRequest();

        bool deleted = await _curriculumService.DeleteCourseSectionAsync(model.Id);

        if (!deleted) return NotFound();

        return RedirectToAction("Details", "Courses", new { id = model.CourseId });
    }

    private bool CanManageCourse(Course course)
    {
        if (User.IsInRole(ApplicationRoles.Admin)) return true;

        if (!User.IsInRole(ApplicationRoles.Instructor)) return false;

        string? currentUserId = _userManager.GetUserId(User);

        return !string.IsNullOrWhiteSpace(currentUserId) && course.InstructorId == currentUserId;
    }
}