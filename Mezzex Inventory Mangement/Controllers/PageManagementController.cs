using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.ViewModels;
using Mezzex_Inventory_Mangement.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [Authorize]
    public class PageManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PageManagementController(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
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
        public IActionResult AssignPagesToRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var pages = _context.Pages.ToList();
            var mappings = _context.PageRoleMappings.ToList();

            // Group pages by controller
            var groupedPages = pages
                .GroupBy(p => p.Url.Split('/')[1]) // Extract controller name
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new AssignPagesToRolesViewModel
            {
                Roles = roles,
                Pages = pages,
                PageRoleMappings = mappings,
                GroupedPagesByController = groupedPages
            };

            return View(viewModel);
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
