using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Services.Order_Service
{
    public class OrderViewModel
    {
        public long Id { get; set; }
        public string? OrderNumber { get; set; }
        public long? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

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

        public List<OrderVendorProgressViewModel> VendorProgress { get; set; } = new();
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

        public long? VendorId { get; set; }

        [MaxLength(200)]
        public string? VendorName { get; set; }

        [MaxLength(150)]
        public string? VendorEmail { get; set; }

        [MaxLength(250)]
        public string? VendorCompanyName { get; set; }
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

    public class UpdateOrderVendorForwardStatusViewModel
    {
        [Required]
        public long OrderId { get; set; }

        public string? UserId { get; set; }

        public long? VendorId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "pending";
    }

    public class UpdateOrderVendorForwardStatusResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresFinalization { get; set; }
        public bool IsLocked { get; set; }
        public bool AlreadyExists { get; set; }
        public VendorDeliveredViewModel? VendorDelivered { get; set; }
    }

    public class FinalizeVendorDeliveredViewModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShipmentStatus { get; set; } = "Pending";

        [MaxLength(100)]
        public string? ShipmentProvider { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(500)]
        public string? ShipmentLiveTrackLink { get; set; }

        [MaxLength(255)]
        public string? ShipmentInfo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ShipmentCharge { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VatAmount { get; set; }
    }

    public class UpdateVendorDeliveredShipmentViewModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShipmentStatus { get; set; } = "Pending";

        [MaxLength(100)]
        public string? ShipmentProvider { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(500)]
        public string? ShipmentLiveTrackLink { get; set; }

        [MaxLength(255)]
        public string? ShipmentInfo { get; set; }
    }

    public class VendorDeliveredViewModel
    {
        public long Id { get; set; }
        public string VendorDeliveredStringId { get; set; } = string.Empty;
        public long OrderId { get; set; }
        public long VendorId { get; set; }
        public string? OrderNumber { get; set; }
        public string? VendorName { get; set; }
        public string? VendorCompanyName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShipmentCharge { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShipmentStatus { get; set; } = "Pending";
        public string? ShipmentProvider { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShipmentLiveTrackLink { get; set; }
        public string? ShipmentInfo { get; set; }
        public bool IsFinalized { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<VendorDeliveredDetailViewModel> Details { get; set; } = new();
    }

    public class VendorDeliveredListItemViewModel : VendorDeliveredViewModel
    {
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class VendorDeliveredDetailViewModel
    {
        public long Id { get; set; }
        public long VendorDeliveredId { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
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
        public long ForwardId { get; set; }
        public long VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string? VendorEmail { get; set; }
        public string? VendorCompanyName { get; set; }
        public string FulfillmentStatus { get; set; } = "pending";
        public DateTime? ForwardedAt { get; set; }
        public string? ForwardedByName { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedByName { get; set; }
        public DateTime? LastCommentAt { get; set; }
        public int TotalComments { get; set; }
        public int UnreadComments { get; set; }
        public bool HasUnreadForward { get; set; }
        public bool HasUnreadStatusUpdate { get; set; }
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
        public bool IsRead { get; set; }
    }

    public class OrderVendorProgressViewModel
    {
        public long ForwardId { get; set; }
        public long VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string? VendorEmail { get; set; }
        public string? VendorCompanyName { get; set; }
        public string FulfillmentStatus { get; set; } = "pending";
        public bool IsLocked { get; set; }
        public long? VendorDeliveredId { get; set; }
        public bool VendorDeliveredFinalized { get; set; }
        public string? VendorDeliveredShipmentStatus { get; set; }
        public DateTime? ForwardedAt { get; set; }
        public string? ForwardedByName { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        public string? StatusUpdatedByName { get; set; }
        public int TotalComments { get; set; }
        public int UnreadComments { get; set; }
        public bool HasUnreadForward { get; set; }
        public bool HasUnreadStatusUpdate { get; set; }
    }

    public class OrderVendorNotificationListViewModel
    {
        public string ViewerRole { get; set; } = "unknown";
        public int UnreadCount { get; set; }
        public List<OrderVendorNotificationViewModel> Items { get; set; } = new();
    }

    public class OrderVendorNotificationViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public long VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string ViewerRole { get; set; } = "unknown";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Status { get; set; }
        public bool IsUnread { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
    }
}
