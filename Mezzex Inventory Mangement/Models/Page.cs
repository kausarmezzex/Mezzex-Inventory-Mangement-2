using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class Page
    {
        [Key]
        public int PageId { get; set; }
        public string Name { get; set; } // e.g., "Dashboard", "Reports"
        public string Url { get; set; } // e.g., "/dashboard", "/reports"
        public ICollection<PageRoleMapping> PageRoleMappings { get; set; } // Many-to-Many Relationship
    }

}
