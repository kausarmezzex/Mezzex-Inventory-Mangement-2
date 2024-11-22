using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [Authorize]
    public class AssignCompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AssignCompanyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var companies = await _context.ManageCompany.ToListAsync();
            var assignments = await _context.UserCompanyAssignments.Include(a => a.SellingChannels).ToListAsync();

            var companySellingChannels = new Dictionary<int, List<SellingChannel>>();
            foreach (var company in companies)
            {
                companySellingChannels[company.CompanyId] = await _context.SellingChannel
                    .Where(sc => sc.CompanyId == company.CompanyId)
                    .ToListAsync();
            }

            var viewModel = new AssignCompanyViewModel
            {
                Users = users,
                Companies = companies,
                CompanySellingChannels = companySellingChannels,
                Assignments = assignments
            };
           
            return View(viewModel);
        }

      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCompanyToUser(AssignCompanyViewModel model)
        {
            if (model.Assignments == null || model.Assignments.Count == 0)
            {
                TempData["ErrorMessage"] = "No data received. Please provide assignment details.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";

            try
            {
                // Get all user IDs from the incoming assignments
                var userIds = model.Assignments.Select(a => a.UserId).Distinct().ToList();

                // Fetch all existing assignments for these users
                var existingAssignments = await _context.UserCompanyAssignments
                    .Where(a => userIds.Contains(a.UserId))
                    .ToListAsync();

                // Process each assignment from the frontend
                foreach (var assignment in model.Assignments)
                {
                    // Validate the company
                    if (assignment.CompanyId == 0 || !await _context.ManageCompany.AnyAsync(c => c.CompanyId == assignment.CompanyId))
                    {
                        continue; // Skip invalid assignments
                    }

                    // Validate and filter selling channels
                    var validSellingChannels = await _context.SellingChannel
                        .Where(sc => sc.CompanyId == assignment.CompanyId && assignment.SellingChannelIds.Contains(sc.SellingChannelId))
                        .Select(sc => sc.SellingChannelId)
                        .ToListAsync();

                    // Check if the assignment already exists
                    var existingAssignment = existingAssignments
                        .FirstOrDefault(a => a.UserId == assignment.UserId && a.CompanyId == assignment.CompanyId);

                    if (existingAssignment != null)
                    {
                        // Update existing assignment
                        existingAssignment.SellingChannelIds = validSellingChannels;
                        existingAssignment.ModifiedBy = currentUserName;
                        existingAssignment.ModifiedOn = DateTime.UtcNow;

                        // Remove from the list of existing assignments (to track what's left for deletion)
                        existingAssignments.Remove(existingAssignment);
                    }
                    else
                    {
                        // Add new assignment
                        var newAssignment = new UserCompanyAssignment
                        {
                            UserId = assignment.UserId,
                            CompanyId = assignment.CompanyId,
                            SellingChannelIds = validSellingChannels,
                            CreatedBy = currentUserName,
                            CreatedOn = DateTime.UtcNow,
                            ModifiedBy = null,
                            ModifiedOn = null
                        };

                        _context.UserCompanyAssignments.Add(newAssignment);
                    }
                }

                // Delete assignments that exist in the database but are not present in the frontend data
                foreach (var leftoverAssignment in existingAssignments)
                {
                    _context.UserCompanyAssignments.Remove(leftoverAssignment);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Selling channels updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to update selling channels. Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AssignmentDetails()
        {
            // Load assignments along with user, company, and selling channels
            var assignments = await _context.UserCompanyAssignments
                .Include(a => a.User)
                .Include(a => a.Company)
                .ToListAsync();

            // Load selling channels for each assignment based on SellingChannelIds
            foreach (var assignment in assignments)
            {
                assignment.SellingChannels = await _context.SellingChannel
                    .Where(sc => assignment.SellingChannelIds.Contains(sc.SellingChannelId))
                    .ToListAsync();
            }

            // Prepare ViewModel
            var viewModel = new AssignCompanyViewModel
            {
                Assignments = assignments
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetAssignmentDetailsByUserId(int userId)
        {
            // Load assignments only for the specific user, including related company and selling channels
            var assignments = await _context.UserCompanyAssignments
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Include(a => a.Company)
                .ToListAsync();

            // Load selling channels for each assignment based on SellingChannelIds
            foreach (var assignment in assignments)
            {
                assignment.SellingChannels = await _context.SellingChannel
                    .Where(sc => assignment.SellingChannelIds.Contains(sc.SellingChannelId))
                    .ToListAsync();
            }

            // Prepare ViewModel with filtered assignments
            var viewModel = new AssignCompanyViewModel
            {
                Assignments = assignments
            };

            // Return a partial view with the assignment details instead of JSON
            return PartialView("_UserAssignmentDetailsPartial", viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            // Load users, companies, and their assignments from the database
            var users = await _context.Users.ToListAsync();
            var companies = await _context.ManageCompany.ToListAsync();
            var assignments = await _context.UserCompanyAssignments
                .Include(a => a.Company)
                .Include(a => a.User)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Assigned Companies");

                // Set headers for the worksheet
                worksheet.Cells[1, 1].Value = "User";
                int companyHeaderIndex = 2;
                foreach (var company in companies)
                {
                    worksheet.Cells[1, companyHeaderIndex++].Value = company.CompanyName;
                }

                int rowIndex = 2;

                // Populate the worksheet with user assignments
                foreach (var user in users)
                {
                    worksheet.Cells[rowIndex, 1].Value = user.UserName; // User name
                    int columnIndex = 2;

                    foreach (var company in companies)
                    {
                        // Get the assignment for the current user and company
                        var userAssignment = assignments
                            .FirstOrDefault(a => a.UserId == user.Id && a.CompanyId == company.CompanyId);

                        if (userAssignment != null)
                        {
                            // Fetch associated selling channels for this assignment
                            var sellingChannels = await _context.SellingChannel
                                .Where(sc => userAssignment.SellingChannelIds.Contains(sc.SellingChannelId))
                                .Select(sc => sc.SellingChannelName)
                                .ToListAsync();

                            worksheet.Cells[rowIndex, columnIndex].Value = string.Join(", ", sellingChannels);
                        }
                        else
                        {
                            worksheet.Cells[rowIndex, columnIndex].Value = ""; // No assignment
                        }
                        columnIndex++;
                    }
                    rowIndex++;
                }

                // Apply styles to headers
                using (var range = worksheet.Cells[1, 1, 1, companies.Count + 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.AutoFitColumns();
                }

                // Save the Excel package to a memory stream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Return the Excel file as a downloadable response
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "AssignedCompanies.xlsx");
            }
        }
    }
}
