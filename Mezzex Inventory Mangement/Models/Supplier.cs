using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mezzex_Inventory_Mangement.Models
{
    public class Supplier : BaseEntity
    {
        [Key]
        public int SupplierId { get; set; } // Primary Key

        [Required]
        [StringLength(50)]
        [Display(Name = "Supplier Type")]
        public string SupplierType { get; set; } // Dropdown Selection

        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [StringLength(100)]
        [Display(Name = "Website")]
        public string WebsiteName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [StringLength(20)]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Representative Name")]
        public string RepresentativeName { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "VAT Cycle")]
        public string VatCycle { get; set; } // Dropdown Selection

        // Navigation Property for Companies
        public ICollection<ManageCompany> Companies { get; set; } = new List<ManageCompany>();

        // Static Lists for Dropdown Options
        public static List<string> VatCycleOptions => new List<string>
        {
            "Weekly Vat",
            "Weekly Vat",
            "Monthly Vat",
            "Quarterly Vat",
            "Six Monthly Vat",
            "Yearly Vat",
            "No Selection"
        };

        public static List<string> SupplierTypeOptions => new List<string> { "Local", "International" };
    }
}
