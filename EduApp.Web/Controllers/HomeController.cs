using System.Diagnostics;
using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly CourseService _courseService;

    public HomeController(CourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        HomeViewModel model = await _courseService.GetHomePageDataAsync();
        return View(model);
    }

    [HttpGet("/hata")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}