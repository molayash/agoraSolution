using CRM.Application.Services.Auth_Service;
using CRM.Application.Services.Order_Service;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Services.Customer_Service
{
    public class CustomerCheckoutRegistrationVm
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required.")]
        [MaxLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
    }

    public class UpdateCustomerProfileVm
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required.")]
        [MaxLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;
    }

    public class CustomerProfileVm
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CustomerRegistrationResultVm
    {
        public string Message { get; set; } = string.Empty;
        public CustomerProfileVm Customer { get; set; } = new();
        public LoginResponseVM Login { get; set; } = new();
    }

    public class CustomerListItemVm
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string LatestOrderStatus { get; set; } = "No orders";
        public DateTime? LatestOrderDate { get; set; }
        public DateTime? JoinedAt { get; set; }
    }

    public class CustomerAccountOverviewVm
    {
        public CustomerProfileVm Customer { get; set; } = new();
        public List<OrderViewModel> Orders { get; set; } = new();
    }
}
