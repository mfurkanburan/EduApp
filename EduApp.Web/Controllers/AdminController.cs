using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminController : Controller
{
    private readonly CourseService _courseService;

    public AdminController(CourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        AdminDashboardViewModel model = await _courseService.GetAdminDashboardAsync();
        return View(model);
    }
}