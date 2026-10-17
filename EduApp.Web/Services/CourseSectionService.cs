using EduApp.Web.Data;
using EduApp.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class CourseSectionService : ICourseSectionService
{
    private readonly ApplicationDbContext _context;

    public CourseSectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseSection?> GetCourseSectionByIdAsync(int id)
    {
        return await _context.CourseSections
            .Include(section => section.Course)
            .Include(section => section.Lessons)
            .AsNoTracking()
            .FirstOrDefaultAsync(section => section.Id == id);
    }

    public async Task<int> GetNextSectionOrderAsync(int courseId)
    {
        int? maximumOrder = await _context.CourseSections
            .Where(section => section.CourseId == courseId)
            .MaxAsync(section => (int?)section.Order);

        return (maximumOrder ?? 0) + 1;
    }

    public async Task CreateCourseSectionAsync(CourseSection courseSection)
    {
        _context.CourseSections.Add(courseSection);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCourseSectionAsync(CourseSection courseSection)
    {
        CourseSection? existingSection = await _context.CourseSections.FirstOrDefaultAsync(existing => existing.Id == courseSection.Id);

        if (existingSection is null) return;

        existingSection.Title = courseSection.Title;
        existingSection.Order = courseSection.Order;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCourseSectionAsync(int id)
    {
        CourseSection? section = await _context.CourseSections.FirstOrDefaultAsync(section => section.Id == id);

        if (section is null) return false;

        int courseId = section.CourseId;

        _context.CourseSections.Remove(section);
        await _context.SaveChangesAsync();

        await ReorderCourseSectionsAsync(courseId);

        return true;
    }

    public async Task ReorderCourseSectionsAsync(int courseId)
    {
        List<CourseSection> sections = await _context.CourseSections
            .Where(section => section.CourseId == courseId)
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id)
            .ToListAsync();

        for (int i = 0; i < sections.Count; i++)
        {
            sections[i].Order = i + 1;
        }

        await _context.SaveChangesAsync();
    }
}