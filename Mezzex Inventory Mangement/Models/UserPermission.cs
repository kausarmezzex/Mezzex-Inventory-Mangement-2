namespace Mezzex_Inventory_Mangement.Models
{
    public class UserPermission
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationUser User { get; set; }
        public PermissionName Permission { get; set; }

        // New properties to link with SellingChannel
        public ApplicationRole Role { get; set; }
        public bool IsOverride { get; set; } // Indicates if it's an override
        public bool IsAssigned { get; internal set; }

    }
}
