using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;

namespace Mezzex_Inventory_Mangement.Controllers
{

    public class RolesController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            this._userManager = userManager;
        }

        // GET: Roles
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";
            role.CreatedBy = currentUserName;
            role.CreatedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Role created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] = "Error occurred while creating the Role.";
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApplicationRole role)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";
            if (id != role.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var existingRole = await _roleManager.FindByIdAsync(id.ToString());
                if (existingRole == null)
                {
                    return NotFound();
                }

                existingRole.Name = role.Name;
                existingRole.ModifiedBy = currentUserName;
                existingRole.ModifiedOn = DateTime.Now;// Update role name
                var result = await _roleManager.UpdateAsync(existingRole);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Role Update successfully!";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] = "Error occurred while updating the Role.";
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Role Delete successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
        // Export to Excel
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var roles = _roleManager.Roles.ToList(); // Fetch all roles

            // Generate Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Roles");

                // Add headers
                var headers = new[]
                {
                    "Role Name", "Created On", "Created By", "Modified On", "Modified By"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Add role data
                for (int i = 0; i < roles.Count; i++)
                {
                    var role = roles[i];
                    var row = i + 2;

                    worksheet.Cells[row, 1].Value = role.Name; // Role Name
                    worksheet.Cells[row, 2].Value = role.CreatedOn?.ToString("yyyy-MM-dd"); // Created On
                    worksheet.Cells[row, 3].Value = role.CreatedBy ?? "N/A"; // Created By
                    worksheet.Cells[row, 4].Value = role.ModifiedOn?.ToString("yyyy-MM-dd"); // Modified On
                    worksheet.Cells[row, 5].Value = role.ModifiedBy ?? "N/A"; // Modified By
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Roles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
