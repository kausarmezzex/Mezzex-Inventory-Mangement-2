using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this._context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            if (!selectedCompanyId.HasValue)
            {
                return RedirectToAction("SelectCompany", "Account");
            }

            var selectedCompany = await _context.ManageCompany
                .Where(c => c.CompanyId == selectedCompanyId.Value)
                .FirstOrDefaultAsync();

            if (selectedCompany == null)
            {
                return RedirectToAction("SelectCompany", "Account");
            }

            // Fetch all companies assigned to the user
            var assignedCompanies = await _context.UserCompanyAssignments
                .Where(a => a.UserId == currentUser.Id)
                 .Include(a => a.Company)
                    .Select(a => a.Company)
                    .ToListAsync();

            HttpContext.Session.SetString("AssignedCompanies", JsonConvert.SerializeObject(assignedCompanies));
            HttpContext.Session.SetString("SelectedCompanyName", selectedCompany.CompanyName); // Store in session

            ViewBag.SelectedCompanyId = selectedCompany.CompanyId;
            ViewBag.SelectedCompanyName = selectedCompany.CompanyName;
            ViewBag.AssignedCompanies = assignedCompanies;

            return View();
        }

        [HttpPost]
        public IActionResult ChangeCompany(int companyId)
        {
            TempData["SelectedCompany"] = companyId;
            return RedirectToAction("Index");
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
