using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class UserAssignment
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsAssigned { get; set; }
        public List<PermissionAssignment> Permissions { get; set; } = new List<PermissionAssignment>();
        public ICollection<RolePermission> RolePermissions { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }

    }

}
