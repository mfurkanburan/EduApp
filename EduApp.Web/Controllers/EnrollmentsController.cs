using EduApp.Web.Models;
using EduApp.Web.Models.Identity;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.Student)]
public class EnrollmentsController : Controller
{
    private readonly EnrollmentService _enrollmentService;
    private readonly CurriculumService _curriculumService;
    private readonly ProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentsController(
        EnrollmentService enrollmentService,
        CurriculumService curriculumService,
        ProfileService profileService,
        UserManager<ApplicationUser> userManager)
    {
        _enrollmentService = enrollmentService;
        _curriculumService = curriculumService;
        _profileService = profileService;
        _userManager = userManager;
    }

    [HttpPost("kurslar/kaydol")]
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

    [HttpGet("kurslarim")]
    public async Task<IActionResult> MyCourses()
    {
        string? studentId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(studentId)) return Challenge();

        List<Enrollment> enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
        List<MyCourseViewModel> model = new();

        foreach (Enrollment enrollment in enrollments)
        {
            if (enrollment.Course is null) continue;

            CourseProgressResult progress = await _enrollmentService.GetCourseProgressAsync(studentId, enrollment.CourseId);

            model.Add(new MyCourseViewModel
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course.Title,
                CategoryName = enrollment.Course.Category?.Name ?? "Kategori bulunamadı",
                InstructorName = enrollment.Course.Instructor?.FullName ?? "Eğitmen bulunamadı",
                CoverImagePath = enrollment.Course.CoverImagePath,
                EnrolledAt = enrollment.EnrolledAt,
                TotalLessonCount = progress.TotalLessonCount,
                CompletedLessonCount = progress.CompletedLessonCount,
                ProgressPercentage = progress.ProgressPercentage,
                IsCompleted = progress.IsCompleted
            });
        }

        return View(model);
    }

    [HttpPost("dersler/tamamla")]
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