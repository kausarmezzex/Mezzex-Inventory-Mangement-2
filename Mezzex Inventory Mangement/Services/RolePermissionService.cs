using Mezzex_Inventory_Mangement.Data;
using Mezzex_Inventory_Mangement.Models;
using Microsoft.EntityFrameworkCore;

namespace Mezzex_Inventory_Mangement.Services
{
    public class RolePermissionService
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add permission for a role to access a page
        public async Task AddPermissionAsync(int roleId, int pageId)
        {
            if (!_context.PageRoleMappings.Any(prm => prm.RoleId == roleId && prm.PageId == pageId))
            {
                var mapping = new PageRoleMapping
                {
                    RoleId = roleId,
                    PageId = pageId
                };

                _context.PageRoleMappings.Add(mapping);
                await _context.SaveChangesAsync();
            }
        }

        // Check if a role has access to a specific page (supports wildcards)
        public async Task<bool> HasAccessToPageAsync(IEnumerable<string> roles, string pageUrl)
        {
            // Get all role IDs associated with the roles
            var roleIds = await _context.Roles
                .Where(r => roles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync(); // Fetch IDs asynchronously

            // Load PageRoleMappings into memory and filter client-side
            var pageMappings = _context.PageRoleMappings
                .Include(prm => prm.Page)
                .Where(prm => roleIds.Contains(prm.RoleId)) // Database-level filtering
                .AsEnumerable() // Switch to in-memory processing
                .ToList(); // Materialize into memory

            // Perform additional string operations in memory
            return pageMappings.Any(prm =>
                prm.Page.Url.Equals(pageUrl, StringComparison.OrdinalIgnoreCase) || // Exact match
                (prm.Page.Url.EndsWith("/*") && pageUrl.StartsWith(prm.Page.Url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase))); // Wildcard match
        }

        // Get all pages accessible to a role
        public async Task<List<Page>> GetAccessiblePagesAsync(int roleId)
        {
            return await _context.PageRoleMappings
                .Where(prm => prm.RoleId == roleId)
                .Select(prm => prm.Page)
                .ToListAsync();
        }

        // Remove a permission for a role to access a page
        public async Task RemovePermissionAsync(int roleId, int pageId)
        {
            var mapping = await _context.PageRoleMappings
                .FirstOrDefaultAsync(prm => prm.RoleId == roleId && prm.PageId == pageId);

            if (mapping != null)
            {
                _context.PageRoleMappings.Remove(mapping);
                await _context.SaveChangesAsync();
            }
        }
    }
}
