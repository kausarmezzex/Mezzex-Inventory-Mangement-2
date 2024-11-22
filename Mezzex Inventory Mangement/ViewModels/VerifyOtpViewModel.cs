using System.ComponentModel.DataAnnotations;

namespace Mezzex_Inventory_Mangement.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Otp { get; set; }
    }

}
