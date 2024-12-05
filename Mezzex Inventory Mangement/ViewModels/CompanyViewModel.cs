namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<SellingChannelViewModel> SellingChannels { get; set; }
    }

    public class SellingChannelViewModel
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
    }

}
