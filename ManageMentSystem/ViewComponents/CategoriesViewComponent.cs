using Microsoft.AspNetCore.Mvc;
using ManageMentSystem.Services.CategoryServices;
using ManageMentSystem.Models;

namespace ManageMentSystem.ViewComponents
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoriesViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return View(categories);
        }
    }
} 