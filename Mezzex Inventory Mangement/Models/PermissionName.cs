namespace Mezzex_Inventory_Mangement.Models
{
    public class PermissionName
    {
        public int Id { get; set; }
        public string Name { get; set; } // Example: "View", "Add", "Edit", "Delete"
        public string? Page { get; set; } // Example: "Users", "Company"

        // Navigation properties
        public ICollection<UserPermission> UserPermissions { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }

}
