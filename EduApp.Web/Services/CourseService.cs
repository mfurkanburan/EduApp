using System.Globalization;
using EduApp.Web.Data;
using EduApp.Web.Models;
using EduApp.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class CourseService
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
            .Include(c => c.Instructor)
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

    public async Task<PagedCoursesResult> GetAllCoursesAsync(
        string? search,
        int? categoryId,
        CourseLevel? level,
        int page,
        int pageSize)
    {
        IQueryable<Course> query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .AsNoTracking();

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
        }

        query = query.OrderByDescending(c => c.CreatedAt);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();

            List<Course> filteredCourses = (await query.ToListAsync())
                .Where(c =>
                    ContainsTurkish(c.Title, term) ||
                    ContainsTurkish(c.Category?.Name, term) ||
                    ContainsTurkish(c.Instructor?.FullName, term))
                .ToList();

            int totalPages = (int)Math.Ceiling(filteredCourses.Count / (double)pageSize);

            return new PagedCoursesResult
            {
                TotalPages = totalPages,
                Courses = filteredCourses
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
            };
        }

        int totalCourses = await query.CountAsync();
        int pages = (int)Math.Ceiling(totalCourses / (double)pageSize);

        List<Course> courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedCoursesResult
        {
            TotalPages = pages,
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

    private static readonly CompareInfo TurkishCompare = CultureInfo.GetCultureInfo("tr-TR").CompareInfo;

    private static bool ContainsTurkish(string? source, string term)
    {
        return !string.IsNullOrWhiteSpace(source)
            && TurkishCompare.IndexOf(source, term, CompareOptions.IgnoreCase) >= 0;
    }
}