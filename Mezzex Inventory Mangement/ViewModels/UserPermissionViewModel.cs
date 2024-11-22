using Mezzex_Inventory_Mangement.Models;

public class UserPermissionViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public List<PermissionAssignment> Permissions { get; set; }
}