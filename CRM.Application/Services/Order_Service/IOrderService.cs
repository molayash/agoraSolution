using CRM.Application.Common.Pagination;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Services.Order_Service
{
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order with items
        /// </summary>
        /// <returns>1 = Success, 0 = Failed</returns>
        Task<int> CreateOrder(OrderViewModel model, CancellationToken ct);

        /// <summary>
        /// Gets all orders
        /// </summary>
        Task<OrderViewModel> GetAllOrders(CancellationToken ct);

        /// <summary>
        /// Gets a single order by ID
        /// </summary>
        Task<OrderViewModel> GetOrderById(long id, CancellationToken ct);

        /// <summary>
        /// Updates order status
        /// </summary>
        /// <returns>2 = Success, 1 = Order not found, 0 = Failed</returns>
        Task<int> UpdateOrderStatus(UpdateOrderStatusViewModel model, CancellationToken ct);

        /// <summary>
        /// Soft deletes an order
        /// </summary>
        Task<bool> DeleteOrder(long id, CancellationToken ct);

        /// <summary>
        /// Gets paginated orders
        /// </summary>
        Task<PaginatedResult<OrderViewModel>> GetOrdersPagination(PaginationRequest request, CancellationToken ct);

        /// <summary>
        /// Gets orders by customer phone
        /// </summary>
        Task<List<OrderViewModel>> GetOrdersByCustomer(string phone, CancellationToken ct);

        /// <summary>
        /// Gets orders by status
        /// </summary>
        Task<List<OrderViewModel>> GetOrdersByStatus(string status, CancellationToken ct);

        /// <summary>
        /// Forwards an order to a vendor via email (simulated)
        /// </summary>
        Task<bool> ForwardToVendor(ForwardOrderViewModel model, CancellationToken ct);
    }
}
