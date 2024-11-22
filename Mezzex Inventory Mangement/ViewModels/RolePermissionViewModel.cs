using Mezzex_Inventory_Mangement.Models;

public class RolePermissionViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public List<PermissionAssignment> Permissions { get; set; } = new List<PermissionAssignment>(); // Initialize with an empty list
}
