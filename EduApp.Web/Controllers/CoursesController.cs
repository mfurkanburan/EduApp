using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;

    public CoursesController(ICourseService courseService, ICategoryService categoryService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;

        PagedCoursesResult result = await _courseService.GetAllCoursesAsync(search, page, pageSize);

        CourseListViewModel model = new CourseListViewModel
        {
            Search = search,
            CurrentPage = page,
            TotalPages = result.TotalPages,
            Courses = result.Courses
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        CreateCourseViewModel model = new CreateCourseViewModel
        {
            Categories = _categoryService.GetAllCategories()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        Course course = new Course
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            CategoryId = model.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _courseService.CreateCourseAsync(course);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        CourseDetailsViewModel model = new CourseDetailsViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            CategoryName = course.Category?.Name ?? "Kategori bulunamadı",
            Sections = course.Sections.Select(s => new SectionViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Order = s.Order,
                Lessons = s.Lessons.Select(l => new LessonViewModel
                {
                    Id = l.Id,
                    Title = l.Title,
                    Order = l.Order
                }).ToList()
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        EditCourseViewModel model = new EditCourseViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCourseViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        Course course = new Course
        {
            Id = model.Id,
            Title = model.Title.Trim(),
            Description = model.Description.Trim()
        };

        await _courseService.UpdateCourseAsync(course);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Course? course = await _courseService.GetCourseByIdAsync(id);

        if (course is null) return NotFound();

        CourseDetailsViewModel model = new CourseDetailsViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CourseDetailsViewModel model)
    {
        await _courseService.DeleteCourseAsync(model.Id);

        return RedirectToAction(nameof(Index));
    }
}