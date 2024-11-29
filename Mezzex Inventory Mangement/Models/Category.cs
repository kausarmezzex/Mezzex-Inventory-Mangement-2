namespace Mezzex_Inventory_Mangement.Models
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public List<Category> SubCategories { get; set; } = new List<Category>();
        public CategoryType CategoryType { get; set; } = CategoryType.Mezzex; // Default to Mezzex

        // Many-to-Many Relationship with Brand
        public ICollection<Brand> Brands { get; set; } = new List<Brand>();
    }

    // Enum to define category types
    public enum CategoryType
    {
        Mezzex,
        Amazon,
        Ebay
    }
}
