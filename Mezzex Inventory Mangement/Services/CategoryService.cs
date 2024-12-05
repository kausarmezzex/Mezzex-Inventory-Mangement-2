using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Data;
using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted) // Exclude deleted categories
                .Include(c => c.SubCategories)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted) // Exclude deleted categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
            if (existingCategory == null)
            {
                return false;
            }

            // Update fields
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.ParentCategoryId = category.ParentCategoryId;
            existingCategory.ModifiedBy = category.ModifiedBy;
            existingCategory.ModifiedOn = category.ModifiedOn;
            existingCategory.IsActive = category.IsActive;
            existingCategory.CategoryType = category.CategoryType;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return false;
            }

            // Set IsDeleted flag
            category.IsDeleted = true;

            // Promote subcategories to main categories
            if (category.SubCategories?.Any() ?? false)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    subCategory.ParentCategoryId = null;
                }
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Category>> GetHierarchicalCategoriesAsync(bool ascending = true)
        {
            var allCategories = await _context.Categories
                .Where(c => !c.IsDeleted && (c.IsActive ?? true)) // Exclude deleted and inactive categories
                .ToListAsync();

            var flattenedCategories = new List<Category>();

            string BuildHierarchyName(Category category, List<Category> allCategories)
            {
                var parentCategory = allCategories.FirstOrDefault(c => c.CategoryId == category.ParentCategoryId);
                if (parentCategory != null)
                {
                    return $"{BuildHierarchyName(parentCategory, allCategories)} >> {category.Name}";
                }
                return category.Name;
            }

            foreach (var category in allCategories)
            {
                var hierarchicalName = BuildHierarchyName(category, allCategories);
                flattenedCategories.Add(new Category
                {
                    CategoryId = category.CategoryId,
                    Name = hierarchicalName,
                    ParentCategoryId = category.ParentCategoryId
                });
            }

            return ascending
                ? flattenedCategories.OrderBy(c => c.Name).ToList()
                : flattenedCategories.OrderByDescending(c => c.Name).ToList();
        }

        public async Task<List<Category>> GetCategoriesByStatusAsync(bool isActive)
        {
            return await _context.Categories
                .Where(c => c.IsActive == isActive && !c.IsDeleted)
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync(bool includeDeleted = true)
        {
            return await _context.Categories
                .Where(c => includeDeleted || !c.IsDeleted)
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }
        public async Task<List<Category>> GetCategoriesByIdsAsync(int[] categoryIds)
        {
            return await _context.Categories
                                 .Where(c => categoryIds.Contains(c.CategoryId) && !c.IsDeleted) // Only include non-deleted categories
                                 .ToListAsync();
        }
    }

}
