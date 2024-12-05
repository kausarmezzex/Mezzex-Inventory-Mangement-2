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
            var companies = await _context.ManageCompany.ToListAsync();

            var companySellingChannels = new Dictionary<int, List<SellingChannel>>();
            foreach (var company in companies)
            {
                companySellingChannels[company.CompanyId] = await _context.SellingChannel
                    .Where(sc => sc.CompanyId == company.CompanyId)
                    .ToListAsync();
            }

            var blockedChannels = await _context.BlockedChannels
                .Where(bc => bc.BrandId == id)
                .ToListAsync();

            ViewBag.Companies = companies;
            ViewBag.Categories = flattenedCategories;
            ViewBag.CompanySellingChannels = companySellingChannels;
            ViewBag.BlockedChannels = blockedChannels;

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
        public async Task<IActionResult> BlockChannels([FromBody] List<BlockedChannelViewModel> blockedChannels)
        {
            if (blockedChannels == null || !blockedChannels.Any())
            {
                return BadRequest("No channels provided.");
            }

            int brandId = blockedChannels.First().BrandId;

            // Remove existing blocked channels for the specified brand
            var existingBlockedChannels = _context.BlockedChannels
                .Where(bc => bc.BrandId == brandId);
            _context.BlockedChannels.RemoveRange(existingBlockedChannels);

            // Add the new blocked channels
            foreach (var channel in blockedChannels)
            {
                var newBlockedChannel = new BlockedChannel
                {
                    CompanyId = channel.CompanyId,
                    BrandId = channel.BrandId,
                    ChannelId = channel.ChannelId,
                    Reason = channel.Reason
                };

                _context.BlockedChannels.Add(newBlockedChannel);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}
