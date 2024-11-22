using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class ManageCompany : BaseEntity
    {
        [Key]
        public int CompanyId { get; set; } // Primary key

        // Company Details
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(100)]
        public string CountryName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string WebsiteName { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(10)]
        public string Postcode { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(20)]
        public string Fax { get; set; }

        [StringLength(50)]
        public string VatNumber { get; set; }

        [StringLength(50)]
        public string CompanyNumber { get; set; }

        [StringLength(10)]
        public string CurrencyCode { get; set; }

        [StringLength(50)]
        public string InvoicePrefixNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string CompanyStatus { get; set; } // Dropdown selection

        [Required]
        [StringLength(20)]
        public string YearStartMonth { get; set; } // Dropdown selection

        [Required]
        [StringLength(20)]
        public string VatCycle { get; set; } // Dropdown selection

        // Bank Details
        [StringLength(100)]
        public string BankName { get; set; }

        [StringLength(100)]
        public string AccountName { get; set; }

        [StringLength(20)]
        public string AccountNumber { get; set; }

        [StringLength(20)]
        public string IFSCCode { get; set; }

        [StringLength(20)]
        public string AccountType { get; set; }

        [StringLength(10)]
        public string SortCode { get; set; }

        [StringLength(34)]
        public string IBAN { get; set; }

        [StringLength(11)]
        public string BIC { get; set; }

        [StringLength(200)]
        public string BankAddress { get; set; }

        // Navigation property for SellingChannels
        public ICollection<SellingChannel> SellingChannels { get; set; } = new List<SellingChannel>();

        // Static lists for dropdown options
        public static List<string> CompanyStatusOptions => new List<string> { "Online", "Offline" };

        public static List<string> YearStartMonthOptions => new List<string>
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

        public static List<string> VatCycleOptions => new List<string> { "Annually", "Quarterly" };

    }


}
