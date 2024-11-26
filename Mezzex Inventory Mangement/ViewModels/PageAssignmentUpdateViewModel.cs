namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class PageAssignmentUpdateViewModel
    {
        public int RoleId { get; set; }
        public string ControllerName { get; set; }
        public List<int> PageIds { get; set; } // Selected Page IDs
    }
}
