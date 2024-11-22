using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mezzex_Inventory_Mangement.Services
{
    public class ManageCompanyService
    {
        private readonly ApplicationDbContext _context;

        public ManageCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all companies
        public async Task<List<ManageCompany>> GetAllCompaniesAsync()
        {
            return await _context.ManageCompany.ToListAsync();
        }

        // Get company by ID
        public async Task<ManageCompany?> GetCompanyByIdAsync(int id)
        {
            return await _context.ManageCompany.FirstOrDefaultAsync(m => m.CompanyId == id);
        }

        // Create new company
        public async Task<bool> CreateCompanyAsync(ManageCompany manageCompany)
        {
            _context.Add(manageCompany);
            return await _context.SaveChangesAsync() > 0;
        }

        // Update existing company
        public async Task<bool> UpdateCompanyAsync(ManageCompany manageCompany)
        {
            _context.Update(manageCompany);
            return await _context.SaveChangesAsync() > 0;
        }

        // Delete company by ID
        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _context.ManageCompany.FindAsync(id);
            if (company != null)
            {
                _context.ManageCompany.Remove(company);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        //// Method to get companies assigned to a specific user
        //public async Task<List<ManageCompany>> GetCompaniesByUserIdAsync(int userId)
        //{
        //    return await _context.ManageCompany
        //                         .Where(company => company.UserId == userId)
        //                         .ToListAsync();
        //}

        // Check if company exists
        public bool CompanyExists(int id)
        {
            return _context.ManageCompany.Any(e => e.CompanyId == id);
        }
    }
}
