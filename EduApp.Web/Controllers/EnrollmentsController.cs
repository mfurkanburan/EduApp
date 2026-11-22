using EduApp.Web.Constants;
using EduApp.Web.Enums;
using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.Services.Results;
using EduApp.Web.ViewModels.Enrollment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.Student)]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILessonProgressService _lessonProgressService;

    public EnrollmentsController(IEnrollmentService enrollmentService, UserManager<ApplicationUser> userManager, ILessonProgressService lessonProgressService)
    {
        _enrollmentService = enrollmentService;
        _userManager = userManager;
        _lessonProgressService = lessonProgressService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        string? studentId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(studentId)) return Challenge();

        EnrollmentResult result = await _enrollmentService.EnrollmentResultAsync(studentId, courseId);

        if (result == EnrollmentResult.CourseNotFound) return NotFound();

        if (result == EnrollmentResult.AlreadyEnrolled)
        {
            TempData["EnrollmentMessage"] = "Zaten bu kursa kayıtlısınız.";
            TempData["EnrollmentMessageType"] = "Warning";

            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        TempData["EnrollmentMessage"] = "Kursa başarıyla kaydoldunuz.";
        TempData["EnrollmentMessageType"] = "Success";

        return RedirectToAction("Details", "Courses", new { id = courseId });
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        string? studentId = _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(studentId)) return Challenge();
        
        List<Enrollment> enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);

        List<MyCourseViewModel> model = new List<MyCourseViewModel>();

        foreach (Enrollment enrollment in enrollments)
        {
            if (enrollment.Course is null) continue;

            CourseProgressResult progressResult = await _lessonProgressService.GetCourseProgressAsync(studentId, enrollment.CourseId);

            model.Add(new MyCourseViewModel
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course.Title,
                CategoryName = enrollment.Course.Category?.Name ?? "Kategori bulunamadı",
                InstructorName = enrollment.Course.Instructor?.FullName ?? "Eğitmen bulunamadı",
                CoverImagePath = enrollment.Course.CoverImagePath,
                EnrolledAt = enrollment.EnrolledAt,
                TotalLessonCount = progressResult.TotalLessonCount,
                CompletedLessonCount = progressResult.CompletedLessonCount,
                ProgressPercentage = progressResult.ProgressPercentage,
                IsCompleted = progressResult.IsCompleted
            });
        }

        return View(model);
    }
}