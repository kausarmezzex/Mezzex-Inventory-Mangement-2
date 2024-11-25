using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class AssignPagesToRolesViewModel
    {
        public List<ApplicationRole> Roles { get; set; }
        public List<Page> Pages { get; set; }
        public List<PageRoleMapping> PageRoleMappings { get; set; }
        public Dictionary<string, List<Page>>? GroupedPagesByController { get; set; } // Grouped Pages by Controller
    }
}
