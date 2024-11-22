namespace Mezzex_Inventory_Mangement.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationRole Role { get; set; }
        public PermissionName Permission { get; set; }
    }
}
