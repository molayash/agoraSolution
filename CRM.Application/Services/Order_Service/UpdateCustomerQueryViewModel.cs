using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Services.Order_Service
{
    public class UpdateCustomerQueryViewModel
    {
        [Required]
        public long Id { get; set; }

        public string? CustomerQuery { get; set; }
    }
}
