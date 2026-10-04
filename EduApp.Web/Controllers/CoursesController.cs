using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public IActionResult Index()
    {
        List<Course> courses = _courseService.GetAllCourses();

        return View(courses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateCourseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Course course = new Course
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim()
        };

        _courseService.CreateCourse(course);

        return RedirectToAction(nameof(Index));
    }
}