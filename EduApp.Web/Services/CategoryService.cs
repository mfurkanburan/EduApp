using EduApp.Web.Data;
using EduApp.Web.Enums;
using EduApp.Web.Models;
using EduApp.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduApp.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<CategoryListItemViewModel>> GetListCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                CourseCount = c.Courses.Count
            })
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        string normalizedName = name.Trim().ToLower();

        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(
                c => c.Name.ToLower() == normalizedName && (!excludeId.HasValue || c.Id != excludeId.Value)
            );
    }

    public async Task CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCategoryAsync(Category category)
    {
        Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

        if (existingCategory is null) return false;

        existingCategory.Name = category.Name;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CategoryDeleteResult> DeleteResultAsync(int id)
    {
        Category? category = await _context.Categories
            .Include(c => c.Courses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return CategoryDeleteResult.NotFound;

        bool hasCourses = await _context.Courses.AnyAsync(c => c.CategoryId == id);

        if (hasCourses) return CategoryDeleteResult.HasCourses;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return CategoryDeleteResult.Deleted;
    }
}