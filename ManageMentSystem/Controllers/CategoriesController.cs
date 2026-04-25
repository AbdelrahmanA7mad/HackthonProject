using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.CategoryServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Categories
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            
            // تطبيق Pagination
            var totalItems = allCategories.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var categories = allCategories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            
            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.CreateCategoryAsync(model);
                    return Json(new { success = true, message = "تم إنشاء الفئة بنجاح" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "حدث خطأ أثناء إنشاء الفئة: " + ex.Message });
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join("<br>", errors) });
        }

        // GET: Categories/Edit/5 (Now returns JSON for modal)
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Json(new { 
                id = category.Id,
                name = category.Name,
                description = category.Description,
                isActive = category.IsActive
            });
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return Json(new { success = false, message = "معرف الفئة غير غير متطابق" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(model);
                    return Json(new { success = true, message = "تم تحديث الفئة بنجاح" });
                }
                catch (ArgumentException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "حدث خطأ أثناء تحديث الفئة: " + ex.Message });
                }
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join("<br>", errors) });
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try 
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "تم حذف الفئة بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "لا يمكن حذف الفئة لوجود منتجات مرتبطة بها" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف: " + ex.Message });
            }
        }

        // GET: Categories/Products/5
        public async Task<IActionResult> Products(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
} 