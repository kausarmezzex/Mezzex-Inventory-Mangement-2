using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Services;
using Mezzex_Inventory_Mangement.Data;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [Authorize]
    public class SellingChannelsController : Controller
    {
        private readonly SellingChannelService _sellingChannelService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public SellingChannelsController(SellingChannelService sellingChannelService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _sellingChannelService = sellingChannelService;
            _context = context;
            _userManager = userManager;
        }

        private void PopulateCompaniesDropdown(int? selectedCompanyId = null)
        {
            ViewData["CompanyId"] = new SelectList(_context.ManageCompany, "CompanyId", "CompanyName", selectedCompanyId);
        }

        public async Task<IActionResult> Index()
        {
            var channels = await _sellingChannelService.GetAllAsync();
            return View(channels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var channel = await _sellingChannelService.GetByIdAsync(id);
            if (channel == null)
            {
                TempData["ErrorMessage"] = "Selling Channel not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(channel);
        }

        public IActionResult Create()
        {
            PopulateCompaniesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SellingChannel sellingChannel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    sellingChannel.CreatedBy = $"{user.FirstName} {user.LastName}";
                }
                sellingChannel.CreatedOn = DateTime.Now;

                if (await _sellingChannelService.CreateAsync(sellingChannel))
                {
                    TempData["SuccessMessage"] = "Selling Channel created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to create Selling Channel.";
            }

            PopulateCompaniesDropdown(sellingChannel.CompanyId);
            return View(sellingChannel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var channel = await _sellingChannelService.GetByIdAsync(id);
            if (channel == null)
            {
                TempData["ErrorMessage"] = "Selling Channel not found.";
                return RedirectToAction(nameof(Index));
            }

            PopulateCompaniesDropdown(channel.CompanyId);
            return View(channel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SellingChannel sellingChannel)
        {
            if (id != sellingChannel.SellingChannelId)
            {
                TempData["ErrorMessage"] = "Selling Channel ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    sellingChannel.ModifiedBy = $"{user.FirstName} {user.LastName}";
                }
                sellingChannel.ModifiedOn = DateTime.Now;

                if (await _sellingChannelService.UpdateAsync(sellingChannel))
                {
                    TempData["SuccessMessage"] = "Selling Channel updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Failed to update Selling Channel.";
            }

            PopulateCompaniesDropdown(sellingChannel.CompanyId);
            return View(sellingChannel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var channel = await _sellingChannelService.GetByIdAsync(id);
            if (channel == null)
            {
                TempData["ErrorMessage"] = "Selling Channel not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(channel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _sellingChannelService.DeleteAsync(id))
            {
                TempData["SuccessMessage"] = "Selling Channel deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete Selling Channel.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var channels = await _sellingChannelService.GetAllChannelsWithCompaniesAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("SellingChannels");

                // Adding header row (start from column 1)
                worksheet.Cells[1, 1].Value = "SellingChannelId";
                worksheet.Cells[1, 2].Value = "SellingChannelName";
                worksheet.Cells[1, 3].Value = "CompanyName";
                worksheet.Cells[1, 4].Value = "CreatedBy";
                worksheet.Cells[1, 5].Value = "CreatedOn";
                worksheet.Cells[1, 6].Value = "ModifiedBy";
                worksheet.Cells[1, 7].Value = "ModifiedOn";

                // Style for header
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Populate data rows
                int row = 2;
                foreach (var channel in channels)
                {
                    worksheet.Cells[row, 1].Value = channel.SellingChannelId;
                    worksheet.Cells[row, 2].Value = channel.SellingChannelName;
                    worksheet.Cells[row, 3].Value = channel.Company?.CompanyName ?? "N/A";
                    worksheet.Cells[row, 4].Value = channel.CreatedBy;
                    worksheet.Cells[row, 5].Value = channel.CreatedOn?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 6].Value = channel.ModifiedBy;
                    worksheet.Cells[row, 7].Value = channel.ModifiedOn?.ToString("yyyy-MM-dd");
                    row++;
                }

                // Auto-fit columns to adjust based on content
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileContent = package.GetAsByteArray();

                // Send the Excel file as a downloadable response
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SellingChannels.xlsx");
            }
        }
    }
}
