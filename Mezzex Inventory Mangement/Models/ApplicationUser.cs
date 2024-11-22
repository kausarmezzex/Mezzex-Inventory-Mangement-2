using Microsoft.AspNetCore.Identity;

namespace Mezzex_Inventory_Mangement.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }

        // New properties
        public string? CountryName { get; set; }
        public ICollection<UserPermission>? UserPermissions { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }
        public ICollection<IdentityUserRole<int>>? UserRoles { get; set; } // Many-to-Many with Roles

        // Directly include BaseEntity properties
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ProfileImageUrl { get; set; }

    }
}
