namespace Mezzex_Inventory_Mangement.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Message { get; set; } // Add this property for custom error messages
    }
}
