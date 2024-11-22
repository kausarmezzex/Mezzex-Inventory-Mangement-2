namespace Mezzex_Inventory_Mangement.Models
{
    public class PermissionAssignment
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Page { get; set; }
        public bool IsAssigned { get; set; }
    }
}
