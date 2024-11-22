using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Mangement.Services
{
    public class SellingChannelService
    {
        private readonly ApplicationDbContext _context;

        public SellingChannelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SellingChannel>> GetAllAsync()
        {
            return await _context.SellingChannel.Include(s => s.Company).ToListAsync();
        }
        public async Task<List<SellingChannel>> GetAllChannelsWithCompaniesAsync()
        {
            return await _context.SellingChannel
                .Include(s => s.Company) // Company details ko include karenge
                .ToListAsync();
        }

        public async Task<SellingChannel?> GetByIdAsync(int id)
        {
            return await _context.SellingChannel.Include(s => s.Company)
                .FirstOrDefaultAsync(m => m.SellingChannelId == id);
        }

        public async Task<bool> CreateAsync(SellingChannel sellingChannel)
        {
            _context.Add(sellingChannel);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(SellingChannel sellingChannel)
        {
            _context.Update(sellingChannel);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sellingChannel = await _context.SellingChannel.FindAsync(id);
            if (sellingChannel != null)
            {
                _context.SellingChannel.Remove(sellingChannel);
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public bool SellingChannelExists(int id)
        {
            return _context.SellingChannel.Any(e => e.SellingChannelId == id);
        }
    }
}
