using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class BlockedChannel
    {
        [Key]
        public int BlockChannelId { get; set; }
        public int CompanyId { get; set; }
        public int BrandId { get; set; }
        public int ChannelId { get; set; }
        public string Reason { get; set; }

    }

}
