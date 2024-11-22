using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Mangement.Services
{
    public class UserCompanyService
    {
        private readonly ApplicationDbContext _context;

        public UserCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserCompany>> GetUserCompaniesAsync(int userId)
        {
            return await _context.UserCompanies
                .Include(uc => uc.Company)
                .ThenInclude(c => c.SellingChannels)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
        }

        public async Task AddUserCompanyAsync(UserCompany userCompany)
        {
            _context.UserCompanies.Add(userCompany);
            await _context.SaveChangesAsync();
        }

        public async Task<UserCompany> GetUserCompanyByIdAsync(int id)
        {
            return await _context.UserCompanies
                .Include(uc => uc.Company)
                .FirstOrDefaultAsync(uc => uc.UserCompanyId == id);
        }

        public async Task UpdateUserCompanyAsync(UserCompany userCompany)
        {
            _context.UserCompanies.Update(userCompany);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserCompanyAsync(int id)
        {
            var userCompany = await _context.UserCompanies.FindAsync(id);
            if (userCompany != null)
            {
                _context.UserCompanies.Remove(userCompany);
                await _context.SaveChangesAsync();
            }
        }
    }
}
