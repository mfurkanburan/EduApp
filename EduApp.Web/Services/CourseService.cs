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
        return await _context.Courses.Include(c => c.Category).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task UpdateCourseAsync(Course course)
    {
        Course? courseToUpdate = await _context.Courses.FindAsync(course.Id);

        if (courseToUpdate is null) return;

        courseToUpdate.Title = course.Title;
        courseToUpdate.Description = course.Description;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        Course? course = await _context.Courses.FindAsync(id);
        
        if (course is null) return;

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
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
}