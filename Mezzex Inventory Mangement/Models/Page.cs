namespace Mezzex_Inventory_Mangement.Models
{
    public class Page
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Dashboard", "Reports"
        public string Url { get; set; } // e.g., "/dashboard", "/reports"
        public ICollection<PageRoleMapping> PageRoleMappings { get; set; } // Many-to-Many Relationship
    }

}
