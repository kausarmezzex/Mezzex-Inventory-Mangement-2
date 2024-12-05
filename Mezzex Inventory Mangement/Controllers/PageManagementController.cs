using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.ViewModels;
using Mezzex_Inventory_Mangement.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [Authorize]
    public class PageManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public PageManagementController(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            this._userManager = userManager;
        }

        // GET: List of Pages
        [HttpGet]
        public IActionResult Index()
        {
            var pages = _context.Pages.ToList();
            return View(pages);
        }

        // GET: Create a New Page
        [HttpGet]
        public IActionResult CreatePage()
        {
            return View();
        }

        // POST: Save New Page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePage(Page page)
        {
            if (ModelState.IsValid)
            {
                _context.Pages.Add(page);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Page created successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to create page. Please check the inputs.";
            return View(page);
        }

        [HttpGet]
        public async Task<IActionResult> AssignPagesToRoles()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User); // Adjust this method to match your user retrieval logic

            // Get the current user's roles
            var userRoleIds = _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToList();

            // Fetch roles excluding "Administrator" and the current user's roles
            var roles = _roleManager.Roles
                .Where(r => r.Name != "Administrator" && !userRoleIds.Contains(r.Id)) // Exclude Administrator and logged-in user's roles
                .ToList();

            // Fetch only the pages assigned to the current user's roles
            var pages = _context.PageRoleMappings
                .Where(prm => userRoleIds.Contains(prm.RoleId)) // Filter pages assigned to the user's roles
                .Select(prm => prm.Page)
                .Distinct() // Avoid duplicates if multiple roles have the same page
                .Where(p => !p.Url.StartsWith("/Account") &&    // Exclude Account-related pages
                            !p.Url.StartsWith("/Home") &&       // Exclude Home-related pages
                            p.Url != "/Home/Privacy")           // Exclude the Privacy page specifically
                .ToList();

            // Fetch all page-role mappings for the view model
            var mappings = _context.PageRoleMappings.ToList();

            // Group the filtered pages by controller (derived from Page.Name)
            var groupedPages = pages
                .GroupBy(p => p.Name.Split('-')[0].Trim()) // Extract controller name
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build the view model
            var viewModel = new AssignPagesToRolesViewModel
            {
                Roles = roles, // Excludes Administrator and logged-in user's roles
                Pages = pages, // Only pages assigned to the logged-in user's roles
                PageRoleMappings = mappings,
                GroupedPagesByController = groupedPages
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePageAssignments([FromBody] List<PageAssignmentUpdateViewModel> changes)
        {
            if (changes == null || !changes.Any())
            {
                return Json(new { success = false, message = "No changes provided." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Identify Account, Home, and Privacy-related pages
                var freeAccessPages = _context.Pages
                    .Where(p => p.Url.StartsWith("/Account") ||  // All Account-related pages
                                p.Url.StartsWith("/Home") ||    // All Home-related pages
                                p.Url == "/Home/Privacy")       // Privacy page
                    .ToList();

                // Get the currently logged-in user's roles
                var user = await _userManager.GetUserAsync(User); // Adjust this to match your user identification logic
                var userRoleIds = _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Select(ur => ur.RoleId)
                    .ToList();

                // Fetch Administrator Role ID
                var administratorRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");
                if (administratorRole == null)
                {
                    return Json(new { success = false, message = "Administrator role not found." });
                }
                var administratorRoleId = administratorRole.Id;

                // Fetch pages assigned to the admin's roles
                var adminAssignedPages = _context.PageRoleMappings
                    .Where(prm => userRoleIds.Contains(prm.RoleId))
                    .Select(prm => prm.PageId)
                    .Distinct()
                    .ToHashSet();

                // Get all Role IDs from the incoming request
                var incomingRoleIds = changes.Select(c => c.RoleId).ToHashSet();

                // Get all Role IDs currently in the database (excluding Administrator and current user's roles)
                var existingRoleIds = _context.PageRoleMappings
                    .Where(m => m.RoleId != administratorRoleId && !userRoleIds.Contains(m.RoleId)) // Exclude Administrator and current user roles
                    .Select(m => m.RoleId)
                    .Distinct()
                    .ToHashSet();

                // Get all roles
                var allRoles = _context.Roles.ToList();

                // Identify Role IDs to delete (present in the database but not in the incoming request, excluding Administrator and current user's roles)
                var rolesToDelete = existingRoleIds
                    .Except(incomingRoleIds)
                    .ToList();

                // Delete all mappings for Role IDs that are not part of the incoming request (excluding Administrator and current user's roles)
                if (rolesToDelete.Any())
                {
                    var mappingsToDelete = _context.PageRoleMappings
                        .Where(m => rolesToDelete.Contains(m.RoleId))
                        .ToList();
                    _context.PageRoleMappings.RemoveRange(mappingsToDelete);
                }

                // Process incoming Role IDs
                foreach (var change in changes)
                {
                    // Skip processing if the role is Administrator or belongs to the current logged-in user
                    if (change.RoleId == administratorRoleId || userRoleIds.Contains(change.RoleId))
                        continue;

                    // Remove all existing mappings for the given RoleId
                    var existingMappings = _context.PageRoleMappings
                        .Where(m => m.RoleId == change.RoleId)
                        .ToList();

                    _context.PageRoleMappings.RemoveRange(existingMappings);

                    // Validate that the admin is authorized to assign these pages
                    if (!change.PageIds.All(pageId => adminAssignedPages.Contains(pageId)))
                    {
                        return Json(new { success = false, message = "You are not authorized to assign one or more selected pages." });
                    }

                    // Add new mappings for the given RoleId if pages are provided
                    if (change.PageIds != null && change.PageIds.Any())
                    {
                        var newMappings = change.PageIds.Select(pageId => new PageRoleMapping
                        {
                            PageId = pageId,
                            RoleId = change.RoleId
                        }).ToList();

                        _context.PageRoleMappings.AddRange(newMappings);
                    }
                }

                // Ensure free access pages are assigned to all roles
                foreach (var role in allRoles)
                {
                    foreach (var page in freeAccessPages)
                    {
                        if (!_context.PageRoleMappings.Any(prm => prm.RoleId == role.Id && prm.PageId == page.PageId))
                        {
                            _context.PageRoleMappings.Add(new PageRoleMapping
                            {
                                RoleId = role.Id,
                                PageId = page.PageId
                            });
                        }
                    }
                }

                // Ensure Administrator retains permissions for all pages
                var allPages = _context.Pages.ToList();
                foreach (var page in allPages)
                {
                    if (!_context.PageRoleMappings.Any(prm => prm.RoleId == administratorRoleId && prm.PageId == page.PageId))
                    {
                        _context.PageRoleMappings.Add(new PageRoleMapping
                        {
                            RoleId = administratorRoleId,
                            PageId = page.PageId
                        });
                    }
                }

                // Save changes and commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Page assignments updated successfully, including free access pages and Administrator's permissions." });
            }
            catch (Exception ex)
            {
                // Rollback the transaction on error
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "An error occurred while updating page assignments.", error = ex.Message });
            }
        }




        // POST: Toggle Role to Page Mapping
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePageRoleMapping(int pageId, int roleId, bool isAssigned)
        {
            if (pageId <= 0 || roleId <= 0)
            {
                return Json(new { success = false, message = "Invalid page or role ID." });
            }

            string successMessage;

            if (isAssigned)
            {
                if (!_context.PageRoleMappings.Any(m => m.PageId == pageId && m.RoleId == roleId))
                {
                    _context.PageRoleMappings.Add(new PageRoleMapping
                    {
                        PageId = pageId,
                        RoleId = roleId
                    });
                    await _context.SaveChangesAsync();
                    successMessage = "Page assigned to role successfully!";
                }
                else
                {
                    successMessage = "This mapping already exists.";
                }
            }
            else
            {
                var mapping = _context.PageRoleMappings.FirstOrDefault(m => m.PageId == pageId && m.RoleId == roleId);
                if (mapping != null)
                {
                    _context.PageRoleMappings.Remove(mapping);
                    await _context.SaveChangesAsync();
                    successMessage = "Page unassigned from role successfully!";
                }
                else
                {
                    successMessage = "No such mapping found.";
                }
            }

            return Json(new { success = true, message = successMessage });
        }

        // GET: View Pages and Roles Mappings
        [HttpGet]
        public IActionResult MapPageToRole()
        {
            var roles = _roleManager.Roles.ToList();
            var pages = _context.Pages.ToList();
            var mappings = _context.PageRoleMappings.ToList();

            var viewModel = new AssignPagesToRolesViewModel
            {
                Roles = roles,
                Pages = pages,
                PageRoleMappings = mappings
            };

            return View(viewModel);
        }

        // POST: Map a Role to a Page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MapPageToRole(int pageId, int roleId)
        {
            if (!_context.PageRoleMappings.Any(m => m.PageId == pageId && m.RoleId == roleId))
            {
                _context.PageRoleMappings.Add(new PageRoleMapping
                {
                    PageId = pageId,
                    RoleId = roleId
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Page successfully assigned to the role.";
            }
            else
            {
                TempData["ErrorMessage"] = "This mapping already exists.";
            }

            return RedirectToAction("MapPageToRole");
        }
    }
}
