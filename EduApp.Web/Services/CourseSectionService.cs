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

    public async Task CreateCourseSectionAsync(CourseSection courseSection)
    {
        _context.CourseSections.Add(courseSection);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextSectionOrderAsync(int courseId)
    {
        int sectionCount = await _context.CourseSections
            .CountAsync(section => section.CourseId == courseId);

        return sectionCount + 1;
    }
}