using EduApp.Web.Data;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Course> GetAllCourses(string? search)
    {
        IQueryable<Course> query = _context.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Title.Contains(search));
        }

        return query.ToList();
    }

    public void CreateCourse(Course course)
    {
        _context.Courses.Add(course);
        _context.SaveChanges();
    }

    public Course? GetCourseById(int id)
    {
        return _context.Courses.AsNoTracking().FirstOrDefault(c => c.Id == id);
    }

    public void UpdateCourse(Course course)
    {
        _context.Courses.Update(course);
        _context.SaveChanges();
    }

    public void DeleteCourse(int id)
    {
        Course? course = _context.Courses.Find(id);
        
        if (course is null) return;

        _context.Courses.Remove(course);
        _context.SaveChanges();
    }
}