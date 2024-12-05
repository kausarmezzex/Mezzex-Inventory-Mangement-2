using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.Models
{
    public class Brand : BaseEntity
    {
        [Key]
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Address fields
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }

        // Many-to-Many Relationship with Category
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
