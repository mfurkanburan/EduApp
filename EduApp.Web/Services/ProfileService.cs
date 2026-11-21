using EduApp.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetEnrolledCourseCountAsync(string userId)
    {
        return await _context.Enrollments.AsNoTracking().CountAsync(enrollment => enrollment.StudentId == userId);
    }

    public async Task<int> GetInstructorCourseCountAsync(string userId)
    {
        return await _context.Courses.AsNoTracking().CountAsync(course => course.InstructorId == userId);
    }
}