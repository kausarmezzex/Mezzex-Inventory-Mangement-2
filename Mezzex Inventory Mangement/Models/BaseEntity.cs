namespace Mezzex_Inventory_Mangement.Models
{
    public abstract class BaseEntity
    {
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

}
