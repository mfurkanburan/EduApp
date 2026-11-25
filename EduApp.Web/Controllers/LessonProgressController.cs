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
    private readonly EnrollmentService _enrollmentService;
    private readonly CurriculumService _curriculumService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ProfileService _profileService;

    public LessonProgressController(
        EnrollmentService enrollmentService,
        CurriculumService curriculumService,
        UserManager<ApplicationUser> userManager,
        ProfileService profileService)
    {
        _enrollmentService = enrollmentService;
        _curriculumService = curriculumService;
        _userManager = userManager;
        _profileService = profileService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteLesson(int lessonId)
    {
        string? studentId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(studentId)) return Challenge();

        Lesson? lesson = await _curriculumService.GetLessonByIdAsync(lessonId);

        if (lesson is null || lesson.CourseSection?.Course is null) return NotFound();

        int courseId = lesson.CourseSection.Course.Id;

        LessonProgressResult result = await _enrollmentService.CompleteLessonAsync(studentId, lessonId);

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
                await _profileService.CheckAndAssignBadgesAsync(studentId);

                TempData["ProgressMessage"] = "Dersi başarıyla tamamladınız!";
                TempData["ProgressMessageType"] = "Success";

                break;
        }

        return RedirectToAction("Details", "Courses", new { id = courseId, lessonId });
    }
}