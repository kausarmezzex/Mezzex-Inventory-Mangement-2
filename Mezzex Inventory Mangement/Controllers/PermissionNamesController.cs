using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class PermissionNamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermissionNamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PermissionNames
        public async Task<IActionResult> Index()
        {
            return View(await _context.PermissionsName.ToListAsync());
        }

        // GET: PermissionNames/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permissionName = await _context.PermissionsName
                .FirstOrDefaultAsync(m => m.Id == id);
            if (permissionName == null)
            {
                return NotFound();
            }

            return View(permissionName);
        }

        // GET: PermissionNames/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PermissionNames/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( PermissionName permissionName)
        {
            if (ModelState.IsValid)
            {
                _context.Add(permissionName);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(permissionName);
        }

        // GET: PermissionNames/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permissionName = await _context.PermissionsName.FindAsync(id);
            if (permissionName == null)
            {
                return NotFound();
            }
            return View(permissionName);
        }

        // POST: PermissionNames/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] PermissionName permissionName)
        {
            if (id != permissionName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(permissionName);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PermissionNameExists(permissionName.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(permissionName);
        }

        // GET: PermissionNames/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permissionName = await _context.PermissionsName
                .FirstOrDefaultAsync(m => m.Id == id);
            if (permissionName == null)
            {
                return NotFound();
            }

            return View(permissionName);
        }

        // POST: PermissionNames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permissionName = await _context.PermissionsName.FindAsync(id);
            _context.PermissionsName.Remove(permissionName);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PermissionNameExists(int id)
        {
            return _context.PermissionsName.Any(e => e.Id == id);
        }


    }
}
