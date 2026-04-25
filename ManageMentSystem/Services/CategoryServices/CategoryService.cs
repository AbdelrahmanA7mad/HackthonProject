using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using ManageMentSystem.Services.UserServices;

namespace ManageMentSystem.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public CategoryService(AppDbContext context, IUserService userService)
        {   
            _context = context;
            _userService = userService;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            var categories = await _context.Categories
                .Where(c => c.TenantId == currentUserId)
                .Include(c => c.Products)
                .OrderByDescending(c => c.Id)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .ToListAsync();

            return categories;
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            var categories = await _context.Categories
                .Where(c => c.IsActive && c.TenantId == currentUserId)
                .Include(c => c.Products)
                .OrderByDescending(c => c.Id)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .ToListAsync();

            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            return await _context.Categories
                .Where(c => c.TenantId == currentUserId)
                .Include(c => c.Products)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(CreateCategoryViewModel model)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                IsActive = true,
                TenantId = tenantId,
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(EditCategoryViewModel model)
        {
            var category = await _context.Categories.FindAsync(model.Id);
            if (category == null)
                throw new ArgumentException("الفئة غير موجودة");

            category.Name = model.Name;
            category.Description = model.Description;
            category.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            // Check if category has products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return false; // Cannot delete category with products

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<int> GetProductsCountByCategoryAsync(int categoryId)
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == currentUserId)
                .CountAsync(p => p.CategoryId == categoryId);
        }
    }
} 
