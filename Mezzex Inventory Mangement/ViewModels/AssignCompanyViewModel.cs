using System.Collections.Generic;
using Mezzex_Inventory_Mangement.Models;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class AssignCompanyViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public List<ManageCompany> Companies { get; set; }
        public Dictionary<int, List<SellingChannel>> CompanySellingChannels { get; set; }
        public List<UserCompanyAssignment> Assignments { get; set; }
    }
}
