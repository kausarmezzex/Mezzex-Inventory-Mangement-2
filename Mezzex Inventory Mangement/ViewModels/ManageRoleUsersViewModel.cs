using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class ManageRoleUsersViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionAssignment> Permissions { get; set; } = new List<PermissionAssignment>();
        public List<UserAssignment> Users { get; set; } = new List<UserAssignment>();
    }

}
