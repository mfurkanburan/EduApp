using EduApp.Web.Enums;
using EduApp.Web.Models;
using EduApp.Web.ViewModels;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();

    Task<List<CategoryListItemViewModel>> GetListCategoriesAsync();

    Task<Category?> GetCategoryByIdAsync(int id);

    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    Task CreateCategoryAsync(Category category);

    Task<bool> UpdateCategoryAsync(Category category);

    Task<CategoryDeleteResult> DeleteResultAsync(int id);
}