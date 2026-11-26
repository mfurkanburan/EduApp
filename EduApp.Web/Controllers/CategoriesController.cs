using EduApp.Web.Models;
using EduApp.Web.Services;
using EduApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public class CategoriesController : Controller
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("yonetim/kategoriler")]
    public async Task<IActionResult> Index()
    {
        List<CategoryListItemViewModel> categories = await _categoryService.GetListCategoriesAsync();
        return View(categories);
    }

    [HttpGet("yonetim/kategoriler/yeni")]
    public IActionResult Create()
    {
        return View(new CreateCategoryViewModel());
    }

    [HttpPost("yonetim/kategoriler/yeni")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model)
    {
        string normalizedName = model.Name.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            bool nameExists = await _categoryService.NameExistsAsync(normalizedName);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(CreateCategoryViewModel.Name), "Bu kategori adı zaten mevcut.");
            }
        }
        else
        {
            ModelState.AddModelError(nameof(CreateCategoryViewModel.Name), "Kategori adı boş olamaz.");
        }

        if (!ModelState.IsValid) return View(model);

        Category category = new Category
        {
            Name = normalizedName
        };

        await _categoryService.CreateCategoryAsync(category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("yonetim/kategoriler/{id:int}/duzenle")]
    public async Task<IActionResult> Edit(int id)
    {
        Category? category = await _categoryService.GetCategoryByIdAsync(id);

        if (category is null) return NotFound();

        EditCategoryViewModel model = new EditCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name
        };

        return View(model);
    }

    [HttpPost("yonetim/kategoriler/duzenle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCategoryViewModel model)
    {
        Category? existingCategory = await _categoryService.GetCategoryByIdAsync(model.Id);

        if (existingCategory is null) return NotFound();

        string normalizedName = model.Name.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            bool nameExists = await _categoryService.NameExistsAsync(normalizedName, model.Id);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(EditCategoryViewModel.Name), "Bu kategori adı zaten mevcut.");
            }
        }
        else
        {
            ModelState.AddModelError(nameof(EditCategoryViewModel.Name), "Kategori adı boş olamaz.");
        }

        if (!ModelState.IsValid) return View(model);

        Category updatedCategory = new Category
        {
            Id = model.Id,
            Name = normalizedName
        };

        bool updated = await _categoryService.UpdateCategoryAsync(updatedCategory);

        if (!updated) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("yonetim/kategoriler/{id:int}/sil")]
    public async Task<IActionResult> Delete(int id)
    {
        Category? category = await _categoryService.GetCategoryByIdAsync(id);

        if (category is null) return NotFound();

        List<CategoryListItemViewModel> categories = await _categoryService.GetListCategoriesAsync();

        CategoryListItemViewModel? categoryItem = categories.FirstOrDefault(c => c.Id == id);

        DeleteCategoryViewModel model = new DeleteCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            CourseCount = categoryItem?.CourseCount ?? 0
        };

        return View(model);
    }

    [HttpPost("yonetim/kategoriler/sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeleteCategoryViewModel model)
    {
        CategoryDeleteResult result = await _categoryService.DeleteResultAsync(model.Id);

        if (result == CategoryDeleteResult.NotFound) return NotFound();

        if (result == CategoryDeleteResult.HasCourses)
        {
            ModelState.AddModelError(string.Empty, "Bu kategoriye ait kurslar bulunduğu için silinemez.");

            Category? category = await _categoryService.GetCategoryByIdAsync(model.Id);

            if (category is null) return NotFound();

            model.Name = category.Name;

            List<CategoryListItemViewModel> categories = await _categoryService.GetListCategoriesAsync();

            model.CourseCount = categories.FirstOrDefault(c => c.Id == model.Id)?.CourseCount ?? 0;

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}