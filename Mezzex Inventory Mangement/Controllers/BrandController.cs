using Microsoft.AspNetCore.Mvc;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.ViewModels;
using Mezzex_Inventory_Mangement.Data;
using System.Drawing.Drawing2D;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class BrandController : Controller
    {
        private readonly BrandService _brandService;
        private readonly CategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BrandController(BrandService brandService, CategoryService categoryService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _brandService = brandService;
            _categoryService = categoryService;
            this._userManager = userManager;
            this._context = context;
        }

        public async Task<IActionResult> Index(string filter = "Active")
        {
            var brands = filter switch
            {
                "Inactive" => await _brandService.GetAllBrandsAsync().ContinueWith(t => t.Result.Where(b => b.IsActive == false).ToList()),
                "All" => await _brandService.GetAllBrandsAsync(),
                _ => await _brandService.GetAllBrandsAsync().ContinueWith(t => t.Result.Where(b => b.IsActive == true).ToList()),
            };

            ViewBag.Filter = filter;
            return View(brands);
        }

        public async Task<IActionResult> Create()
        {
            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            ViewBag.Categories = flattenedCategories;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand, int[] selectedCategories)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                brand.CreatedBy = $"{user.FirstName} {user.LastName}";
            }
            brand.CreatedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                if (selectedCategories != null && selectedCategories.Any())
                {
                    brand.Categories = await _categoryService.GetCategoriesByIdsAsync(selectedCategories);
                }

                await _brandService.AddBrandAsync(brand);
                return RedirectToAction(nameof(Index));
            }

            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            ViewBag.Categories = flattenedCategories;
            return View(brand);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            // Fetch all companies for selection
            ViewBag.Companies = _context.ManageCompany.ToList();

            // Fetch blocked companies for this brand
            ViewBag.BlockedCompanies = _context.BlockedCompanies
                .Where(bc => bc.BrandId == id)
                .ToList();
            ViewBag.Categories = flattenedCategories;
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Brand brand, int[] selectedCategories)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                brand.ModifiedBy = $"{user.FirstName} {user.LastName}";
            }
            brand.ModifiedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                // Get selected categories from database
                brand.Categories = await _categoryService.GetCategoriesByIdsAsync(selectedCategories);

                // Update the brand and its categories
                await _brandService.UpdateBrandAsync(brand);

                return RedirectToAction(nameof(Index));
            }

            // Reload categories in case of validation errors
            var flattenedCategories = await _categoryService.GetHierarchicalCategoriesAsync(ascending: true);
            ViewBag.Categories = flattenedCategories;
            return View(brand);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, [FromForm] string confirmationToken = null)
        {
            await _brandService.SoftDeleteBrandAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            return View(brand);
        }
        [HttpPost]
        public async Task<IActionResult> BlockCompanies(BlockCompanyRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (request == null || request.BlockedCompanies == null || !request.BlockedCompanies.Any())
            {
                return BadRequest("Invalid request data.");
            }

            // Remove existing blocked companies for this brand
            var existingBlockedCompanies = _context.BlockedCompanies
                .Where(bc => bc.BrandId == request.BrandId)
                .ToList();
            _context.BlockedCompanies.RemoveRange(existingBlockedCompanies);

            // Add new blocked companies
            foreach (var block in request.BlockedCompanies)
            {
                if (block.CompanyId != 0) // Ensure valid company ID
                {
                    var blockedCompany = new BlockedCompany
                    {
                        BrandId = request.BrandId,
                        CompanyId = block.CompanyId,
                        Reason = block.Reason,
                        CreatedOn = DateTime.Now,
                        CreatedBy = $"{user.FirstName} {user.LastName}",
                    };

                    _context.BlockedCompanies.Add(blockedCompany);
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
