using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
