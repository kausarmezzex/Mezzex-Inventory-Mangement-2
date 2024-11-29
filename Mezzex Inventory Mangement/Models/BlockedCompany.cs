namespace Mezzex_Inventory_Mangement.Models
{
    public class BlockedCompany : BaseEntity
    {
        public int Id { get; set; }

        // Foreign Keys
        public int BrandId { get; set; }
        public int CompanyId { get; set; }

        // Reason for blocking
        public string Reason { get; set; }

        // Navigation Properties
        public Brand Brand { get; set; }
        public ManageCompany Company { get; set; }
    }
}
