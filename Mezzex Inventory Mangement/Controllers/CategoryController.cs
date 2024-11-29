using Microsoft.AspNetCore.Mvc;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Services;
using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoryController(CategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string filter = "Active")
        {
            List<Category> categories;

            switch (filter)
            {
                case "Inactive":
                    categories = await _categoryService.GetCategoriesByStatusAsync(false);
                    break;
                case "All":
                    categories = await _categoryService.GetAllCategoriesAsync(includeDeleted: false);
                    break;
                default:
                    categories = await _categoryService.GetCategoriesByStatusAsync(true);
                    break;
            }

            ViewBag.Filter = filter;
            return View(categories);
        }


        public async Task<IActionResult> Create()
        {
            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            ViewBag.ParentCategories = flattenedCategories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                category.CreatedBy = $"{user.FirstName} {user.LastName}";
            }
            category.CreatedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                await _categoryService.AddCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }

            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            ViewBag.ParentCategories = flattenedCategories;
            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: false);
            ViewBag.ParentCategories = flattenedCategories
               .Where(c => c.Id != id) // Exclude the current category and inactive ones
                .ToList();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                category.ModifiedBy = $"{user.FirstName} {user.LastName}";
            }
            category.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateCategoryAsync(category);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to update category.");
            }

            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: false);
            ViewBag.ParentCategories = flattenedCategories
                .Where(c => c.Id != id) // Exclude the current category and inactive ones
                .ToList();
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category); // Return the confirmation view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, [FromForm] string confirmationToken = null) 
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);
            if (!result)
            {
                ModelState.AddModelError("", "Unable to delete category. Ensure it has no subcategories or dependencies.");
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View("Delete", category);
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
