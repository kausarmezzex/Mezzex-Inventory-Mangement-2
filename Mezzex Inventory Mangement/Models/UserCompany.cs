using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class UserCompany
    {
        [Key]
        public int UserCompanyId { get; set; }

        // Foreign keys
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } // Navigation property to the user

        public int CompanyId { get; set; }
        public ManageCompany Company { get; set; } // Navigation property to the company
    }
}
