using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class SellingChannel : BaseEntity
    {
        [Key]
        public int SellingChannelId { get; set; }    // Selling Channel ki unique ID

        [Required]
        [StringLength(100)]
        public string SellingChannelName { get; set; } // Selling Channel ka naam

        // Foreign key to ManageCompany
        public int CompanyId { get; set; } // Yeh ManageCompany table ko refer karega

        // Navigation property
        public ManageCompany? Company { get; set; } // Company ke details ka access
    }


}
 