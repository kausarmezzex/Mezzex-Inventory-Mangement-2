namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public List<string> Roles { get; set; }
        // Directly include BaseEntity properties
        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

}
