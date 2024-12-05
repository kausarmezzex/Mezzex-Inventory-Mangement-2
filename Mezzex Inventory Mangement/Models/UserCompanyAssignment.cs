using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class UserCompanyAssignment : BaseEntity
    {
        [Key]
        public int UserCompanyAssignmentId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public List<int> SellingChannelIds { get; set; } = new List<int>();  // Stores selected SellingChannel IDs

        // Navigation properties
        public ApplicationUser User { get; set; }
        public ManageCompany Company { get; set; }
        public ICollection<SellingChannel> SellingChannels { get; set; }
    }
}
