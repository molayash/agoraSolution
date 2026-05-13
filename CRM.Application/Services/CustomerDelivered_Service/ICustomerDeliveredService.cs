using CRM.Application.Common.Pagination;
using CRM.Application.Services.Order_Service;
namespace CRM.Application.Services.CustomerDelivered_Service
{
    public interface ICustomerDeliveredService
    {
        Task<UpdateOrderVendorForwardStatusResultViewModel> MarkVendorDeliveredAsync(
            long orderId,
            long vendorId,
            string? actorUserId,
            string actorName,
            bool isVendorActor,
            CancellationToken ct);
        Task<PaginatedResult<CustomerDeliveredListItemViewModel>> GetListAsync(
            PaginationRequest request,
            string? shipmentStatus,
            bool? isFinalized,
            string? userId,
            CancellationToken ct);
        Task<CustomerDeliveredViewModel?> GetByOrderAsync(long orderId, CancellationToken ct);
        Task<CustomerDeliveredViewModel?> GetByOrderVendorAsync(long orderId, long vendorId, CancellationToken ct);
        Task<CustomerDeliveredViewModel?> FinalizeAsync(FinalizeCustomerDeliveredViewModel model, CancellationToken ct);
    }
}
