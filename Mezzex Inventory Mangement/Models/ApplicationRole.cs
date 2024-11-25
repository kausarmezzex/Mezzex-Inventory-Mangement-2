using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ModifiedBy { get; set; }
        public ICollection<IdentityUserRole<int>>? UserRoles { get; set; } // Many-to-Many with Users
    }
}
