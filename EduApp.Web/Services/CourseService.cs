using EduApp.Web.Data;
using EduApp.Web.Models;
using EduApp.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateCourseAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Requirements)
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> UpdateCourseAsync(Course course)
    {
        Course? existingCourse = await _context.Courses
            .Include(c => c.Requirements)
            .FirstOrDefaultAsync(c => c.Id == course.Id);

        if (existingCourse is null) return false;

        existingCourse.Title = course.Title;
        existingCourse.Description = course.Description;
        existingCourse.CategoryId = course.CategoryId;
        existingCourse.Level = course.Level;
        existingCourse.CoverImagePath = course.CoverImagePath;

        _context.CourseRequirements.RemoveRange(existingCourse.Requirements);

        existingCourse.Requirements = course.Requirements;

        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        Course? course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        
        if (course is null) return false;

        _context.Courses.Remove(course);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PagedCoursesResult> GetAllCoursesAsync(string? search, int page, int pageSize)
    {
        IQueryable<Course> query = _context.Courses.Include(c => c.Category).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.Contains(search));
        }

        int totalCourses = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);

        List<Course> courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedCoursesResult
        {
            TotalPages = totalPages,
            Courses = courses
        };
    }

    public async Task<Course?> GetCourseForDeleteAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}