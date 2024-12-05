using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Data;

namespace Mezzex_Inventory_Mangement.Services
{
    public class BrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands
                                 .Where(b => !b.IsDeleted) // Exclude soft-deleted brands
                                 .Include(b => b.Categories)
                                 .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _context.Brands
       .Include(b => b.Categories) // Include related Categories
       .FirstOrDefaultAsync(b => b.BrandId == id);
        }

        public async Task AddBrandAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            var existingBrand = await _context.Brands
                .Include(b => b.Categories) // Include Categories relationship
                .FirstOrDefaultAsync(b => b.BrandId == brand.BrandId);

            if (existingBrand == null) return false;

            // Update fields
            existingBrand.BrandName = brand.BrandName;
            existingBrand.Description = brand.Description;
            existingBrand.Country = brand.Country;
            existingBrand.State = brand.State;
            existingBrand.PhoneNumber = brand.PhoneNumber;
            existingBrand.Website = brand.Website;
            existingBrand.Email = brand.Email;
            existingBrand.IsActive = brand.IsActive;
            existingBrand.ModifiedOn = DateTime.Now;
            existingBrand.ModifiedBy = brand.ModifiedBy;

            // Update Categories
            // Clear existing relationships
            existingBrand.Categories.Clear();

            // Attach new relationships
            if (brand.Categories != null && brand.Categories.Any())
            {
                foreach (var category in brand.Categories)
                {
                    var existingCategory = await _context.Categories.FindAsync(category.CategoryId);

                    if (existingCategory != null)
                    {
                        existingBrand.Categories.Add(existingCategory); // Add existing category
                    }
                }
            }

            _context.Brands.Update(existingBrand);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<bool> SoftDeleteBrandAsync(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null) return false;

            brand.IsDeleted = true;
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task SaveBlockedChannel(int companyId, int brandId, int channelId, string reason)
        {
            // Check if the blocked channel already exists
            var existingBlockedChannel = await _context.BlockedChannels
                .FirstOrDefaultAsync(bc => bc.CompanyId == companyId && bc.BrandId == brandId && bc.ChannelId == channelId);

            if (existingBlockedChannel != null)
            {
                // Update the reason
                existingBlockedChannel.Reason = reason;
                _context.BlockedChannels.Update(existingBlockedChannel);
            }
            else
            {
                // Add new blocked channel
                var newBlockedChannel = new BlockedChannel
                {
                    CompanyId = companyId,
                    BrandId = brandId,
                    ChannelId = channelId,
                    Reason = reason
                };

                _context.BlockedChannels.Add(newBlockedChannel);
            }

            await _context.SaveChangesAsync();
        }

    }
}
