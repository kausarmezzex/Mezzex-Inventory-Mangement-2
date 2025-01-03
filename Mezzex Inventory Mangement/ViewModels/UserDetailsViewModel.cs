﻿namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string CountryName { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permissions { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        // Add new properties for companies and selling channels
        public List<CompanyDetailsViewModel> AssignedCompanies { get; set; } = new List<CompanyDetailsViewModel>();
        public string? PhoneNumber { get; internal set; }
    }

    public class CompanyDetailsViewModel
    {
        public string CompanyName { get; set; }
        public List<string> SellingChannels { get; set; } = new List<string>();
    }

}
