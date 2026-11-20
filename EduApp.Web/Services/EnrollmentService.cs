using EduApp.Web.Data;
using EduApp.Web.Enums;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEnrolledAsync(string studentId, int courseId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<EnrollmentResult> EnrollmentResultAsync(string studentId, int courseId)
    {
        bool courseExists = await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Id == courseId);
        
        if (!courseExists) return EnrollmentResult.CourseNotFound;

        bool alreadyEnrolled = await _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (alreadyEnrolled) return EnrollmentResult.AlreadyEnrolled;

        Enrollment enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);

        await _context.SaveChangesAsync();

        return EnrollmentResult.Enrolled;
    }

    public async Task<List<Enrollment>> GetStudentEnrollmentsAsync(string studentId)
    {
        return await _context.Enrollments
            .Include(enrollment => enrollment.Course)
            .ThenInclude(course => course!.Category)
            .Include(enrollment => enrollment.Course)
            .ThenInclude(course => course!.Instructor)
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId)
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .ToListAsync();
    }
}