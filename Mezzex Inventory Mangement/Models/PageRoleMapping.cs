namespace Mezzex_Inventory_Mangement.Models
{
    public class PageRoleMapping
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public Page Page { get; set; }

        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; }
    }

}
     