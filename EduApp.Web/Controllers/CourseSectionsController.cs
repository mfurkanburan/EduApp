using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class CourseSectionsController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICourseSectionService _courseSectionService;

    public CourseSectionsController(ICourseService courseService, ICourseSectionService courseSectionService)
    {
        _courseService = courseService;
        _courseSectionService = courseSectionService;
    }

    [HttpGet]
    [Route("Courses/{courseId:int}/Sections/Create")]
    public async Task<IActionResult> Create(int courseId)
    {
        Course? course = await _courseService.GetCourseByIdAsync(courseId);

        if (course is null) return NotFound();

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

        if (!ModelState.IsValid) return View(model);

        int nextOrder = await _courseSectionService.GetNextSectionOrderAsync(courseId);

        CourseSection newSection = new CourseSection
        {
            Title = model.Title,
            Order = nextOrder,
            CourseId = courseId
        };

        await _courseSectionService.CreateCourseSectionAsync(newSection);

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
}