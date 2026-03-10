using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Services.Order_Service
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public string? OrderNumber { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(500)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [MaxLength(20)]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [MaxLength(100)]
        public string Country { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ShippingFee { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Tax { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        [MaxLength(2000)]
        public string? CustomerQuery { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();

        // For list response
        public IQueryable<OrderViewModel>? OrderList { get; set; }
    }

    public class OrderItemViewModel
    {
        public long Id { get; set; }
        
        [Required]
        public long ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateOrderStatusViewModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
    }

    public class OrderListResponseViewModel
    {
        public List<OrderViewModel> Data { get; set; } = new List<OrderViewModel>();
        public int TotalCount { get; set; }
    }

    public class ForwardOrderViewModel
    {
        [Required]
        public long OrderId { get; set; }
        
        public string? UserId { get; set; }
        
        [Required]
        public string VendorId { get; set; }
        
        [Required]
        [EmailAddress]
        public string VendorEmail { get; set; }
        
        [Required]
        public string Message { get; set; }
    }

    public class CreateOrderVendorCommentViewModel
    {
        [Required]
        public long OrderId { get; set; }

        public string? UserId { get; set; }

        public long? VendorId { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;
    }

    public class OrderVendorCommentResponseViewModel
    {
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string ViewerRole { get; set; } = "unknown";
        public List<OrderVendorCommentThreadViewModel> Threads { get; set; } = new();
    }

    public class OrderVendorCommentThreadViewModel
    {
        public long VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string? VendorEmail { get; set; }
        public string? VendorCompanyName { get; set; }
        public DateTime? ForwardedAt { get; set; }
        public string? ForwardedByName { get; set; }
        public DateTime? LastCommentAt { get; set; }
        public int TotalComments { get; set; }
        public bool CanComment { get; set; } = true;
        public List<OrderVendorCommentViewModel> Comments { get; set; } = new();
    }

    public class OrderVendorCommentViewModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long VendorId { get; set; }
        public string? SenderUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = "admin";
        public string Message { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
