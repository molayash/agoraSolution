using CRM.Application.Common.Pagination;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Services.Order_Service
{
    public interface IOrderService
    {
        Task<int> CreateOrder(OrderViewModel model, CancellationToken ct);
        Task<OrderViewModel> GetAllOrders(CancellationToken ct);
        Task<OrderViewModel> GetOrderById(long id, CancellationToken ct);
        Task<int> UpdateOrderStatus(UpdateOrderStatusViewModel model, CancellationToken ct);
        Task<OrderViewModel?> UpdateOrderShipment(UpdateOrderShipmentViewModel model, CancellationToken ct);
        Task<bool> UpdateCustomerQuery(UpdateCustomerQueryViewModel model, CancellationToken ct);
        Task<bool> DeleteOrder(long id, CancellationToken ct);
        Task<PaginatedResult<OrderViewModel>> GetOrdersPagination(PaginationRequest request, CancellationToken ct);
        Task<List<OrderViewModel>> GetOrdersByCustomer(string phone, CancellationToken ct);
        Task<List<OrderViewModel>> GetOrdersByCustomerUserId(string userId, CancellationToken ct);
        Task<List<OrderViewModel>> GetOrdersByStatus(string status, CancellationToken ct);
        Task<List<OrderViewModel>> GetMyOrders(string userId, CancellationToken ct);
        Task<AutoForwardOrderResultViewModel> AutoForwardOrderToVendors(long orderId, CancellationToken ct);
        Task<bool> ForwardToVendor(ForwardOrderViewModel model, CancellationToken ct);
        Task<OrderVendorCommentResponseViewModel> GetForwardComments(long orderId, string? userId, bool markAsRead, CancellationToken ct);
        Task<bool> AddForwardComment(CreateOrderVendorCommentViewModel model, CancellationToken ct);
        Task<UpdateOrderVendorForwardStatusResultViewModel> UpdateForwardStatus(UpdateOrderVendorForwardStatusViewModel model, CancellationToken ct);
        Task<OrderVendorNotificationListViewModel> GetForwardNotifications(string? userId, bool markAsRead, CancellationToken ct);
    }
}
