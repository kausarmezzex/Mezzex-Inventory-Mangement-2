namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class BlockCompanyRequest
    {
        public int BrandId { get; set; }
        public List<BlockedCompanyDetails> BlockedCompanies { get; set; }
    }

    public class BlockedCompanyDetails
    {
        public int CompanyId { get; set; }
        public string Reason { get; set; }
    }

}
