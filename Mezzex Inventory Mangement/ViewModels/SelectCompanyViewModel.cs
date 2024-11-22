using Mezzex_Inventory_Mangement.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class SelectCompanyViewModel
    {
        public List<ManageCompany> Companies { get; set; }
        public int SelectedCompanyId { get; set; }
    }
}
