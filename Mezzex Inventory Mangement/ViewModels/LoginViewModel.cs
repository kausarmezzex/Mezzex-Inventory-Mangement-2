using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or phone number is required")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string? PhoneNumber { get; set; } // For internal use
    }

}
