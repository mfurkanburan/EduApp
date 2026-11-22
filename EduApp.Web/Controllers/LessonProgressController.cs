using EduApp.Web.Constants;
using EduApp.Web.Enums;
using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.Student)]
public class LessonProgressController : Controller
{
    private readonly ILessonProgressService _lessonProgressService;
    private readonly ILessonService _lessonService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LessonProgressController(ILessonProgressService lessonProgressService, ILessonService lessonService, UserManager<ApplicationUser> userManager)
    {
        _lessonProgressService = lessonProgressService;
        _lessonService = lessonService;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteLesson(int lessonId)
    {
        string? studentId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(studentId)) return Challenge();

        Lesson? lesson = await _lessonService.GetLessonByIdAsync(lessonId);

        if (lesson is null || lesson.CourseSection?.Course is null) return NotFound();

        int courseId = lesson.CourseSection.Course.Id;

        LessonProgressResult result = await _lessonProgressService.CompleteLessonAsync(studentId, lessonId);

        switch (result)
        {
            case LessonProgressResult.LessonNotFound:
                return NotFound();
            case LessonProgressResult.NotEnrolled:
                return Forbid();
            case LessonProgressResult.AlreadyCompleted:
                TempData["ProgressMessage"] = "Bu dersi daha önce tamamladınız.";
                TempData["ProgressMessageType"] = "Warning";

                break;
            case LessonProgressResult.Completed:
                TempData["ProgressMessage"] = "Dersi başarıyla tamamladınız!";
                TempData["ProgressMessageType"] = "Success";

                break;
        }

        return RedirectToAction("Details", "Courses", new { id = courseId, lessonId });
    }
}