namespace CRM.Application.Services.Vendor_Service
{
    public class VendorCreateResultVm
    {
        public long VendorId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
