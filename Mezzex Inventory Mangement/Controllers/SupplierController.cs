using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Mezzex_Inventory_Mangement.Data;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Management.Controllers
{
    public class SupplierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupplierController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }

        public IActionResult Index(string filter = "Active")
        {
            IQueryable<Supplier> query = _context.Suppliers.AsNoTracking().Include(s => s.Companies);

            if (filter == "Active")
            {
                query = query.Where(s => s.IsDeleted == false && (s.IsActive ?? false)); // IsActive defaulted to false
            }
            else if (filter == "Inactive")
            {
                query = query.Where(s => s.IsDeleted == false && !(s.IsActive ?? false)); // IsActive defaulted to false
            }
            else if (filter == "All")
            {
                query = query.Where(s => s.IsDeleted == false); // Includes both active and inactive
            }

            var suppliers = query.ToList();
            ViewBag.Filter = filter;

            return View(suppliers);
        }

        // GET: Supplier/Create
        public async Task<IActionResult> Create()
        {
            // Example company data fetched from database
            var companies = _context.ManageCompany.Select(c => new
            {
                Id = c.CompanyId,
                Name = c.CompanyName
            }).ToList();
            ViewBag.Companies = companies;
            ViewBag.VatCycleOptions = Supplier.VatCycleOptions;
            ViewBag.SupplierTypeOptions = Supplier.SupplierTypeOptions;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier, int[] selectedCompanyIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                supplier.CreatedBy = $"{user.FirstName} {user.LastName}";
                supplier.CreatedOn = DateTime.Now;
            }

            // Trim all input fields
            supplier.SupplierName = supplier.SupplierName?.Trim();
            supplier.Email = supplier.Email?.Trim();
            supplier.Phone = supplier.Phone?.Trim();
            supplier.WebsiteName = supplier.WebsiteName?.Trim();
            supplier.Password = supplier.Password?.Trim();
            supplier.AccountNumber = supplier.AccountNumber?.Trim();
            supplier.RepresentativeName = supplier.RepresentativeName?.Trim();
            supplier.Address = supplier.Address?.Trim();
            supplier.VatCycle = supplier.VatCycle?.Trim();
            supplier.SupplierType = supplier.SupplierType?.Trim();

            if (ModelState.IsValid)
            {
                var selectedCompanies = await _context.ManageCompany
                    .Where(c => selectedCompanyIds.Contains(c.CompanyId))
                    .ToListAsync();
                supplier.Companies = selectedCompanies;

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VatCycleOptions = Supplier.VatCycleOptions;
            ViewBag.SupplierTypeOptions = Supplier.SupplierTypeOptions;
            ViewBag.Companies = await _context.ManageCompany.ToListAsync();
            return View(supplier);
        }

        // GET: Supplier/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Companies) // Include Companies to prefill selected options
                .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier == null || supplier.IsDeleted) return NotFound();

            ViewBag.VatCycleOptions = Supplier.VatCycleOptions;
            ViewBag.SupplierTypeOptions = Supplier.SupplierTypeOptions;
            ViewBag.Companies = _context.ManageCompany
                .Select(c => new { c.CompanyId, c.CompanyName })
                .ToList();

            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier, int[] selectedCompanyIds)
        {
            if (id != supplier.SupplierId) return BadRequest();

            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid)
            {
                var existingSupplier = await _context.Suppliers
                    .Include(s => s.Companies)
                    .FirstOrDefaultAsync(s => s.SupplierId == id);

                if (existingSupplier == null || existingSupplier.IsDeleted) return NotFound();

                // Trim all input fields
                supplier.SupplierName = supplier.SupplierName?.Trim();
                supplier.Email = supplier.Email?.Trim();
                supplier.Phone = supplier.Phone?.Trim();
                supplier.WebsiteName = supplier.WebsiteName?.Trim();
                supplier.Password = supplier.Password?.Trim();
                supplier.AccountNumber = supplier.AccountNumber?.Trim();
                supplier.RepresentativeName = supplier.RepresentativeName?.Trim();
                supplier.Address = supplier.Address?.Trim();
                supplier.VatCycle = supplier.VatCycle?.Trim();
                supplier.SupplierType = supplier.SupplierType?.Trim();
                // Update properties
                existingSupplier.SupplierType = supplier.SupplierType;
                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.WebsiteName = supplier.WebsiteName;
                existingSupplier.Password = supplier.Password;
                existingSupplier.AccountNumber = supplier.AccountNumber;
                existingSupplier.RepresentativeName = supplier.RepresentativeName;
                existingSupplier.Address = supplier.Address;
                existingSupplier.VatCycle = supplier.VatCycle;
                existingSupplier.ModifiedBy = $"{user.FirstName} {user.LastName}";
                existingSupplier.ModifiedOn = DateTime.Now;
                existingSupplier.IsActive = supplier.IsActive;

                // Update companies
                existingSupplier.Companies.Clear();
                var selectedCompanies = await _context.ManageCompany
                    .Where(c => selectedCompanyIds.Contains(c.CompanyId))
                    .ToListAsync();
                existingSupplier.Companies = selectedCompanies;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VatCycleOptions = Supplier.VatCycleOptions;
            ViewBag.SupplierTypeOptions = Supplier.SupplierTypeOptions;
            ViewBag.Companies = _context.ManageCompany
                .Select(c => new { c.CompanyId, c.CompanyName })
                .ToList();

            return View(supplier);
        }

        // GET: Supplier/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var supplier = _context.Suppliers
       .Include(s => s.Companies) // Include related Companies
       .FirstOrDefault(s => s.SupplierId == id);
            if (supplier == null || supplier.IsDeleted) return NotFound();
            return View(supplier);
        }

        // GET: Supplier/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Companies)
                .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier == null || supplier.IsDeleted)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Supplier/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, [FromForm] string confirmationToken = null)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound(); 
            }

            // Perform a soft delete by marking IsDeleted as true
            supplier.IsDeleted = true;
            supplier.ModifiedOn = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
