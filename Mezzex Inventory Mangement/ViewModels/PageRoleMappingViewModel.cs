using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class PageRoleMappingViewModel
    {
        public List<ApplicationRole> Roles { get; set; }
        public List<Page> Pages { get; set; }
        public List<PageRoleMapping> Mappings { get; set; }
    }
}
