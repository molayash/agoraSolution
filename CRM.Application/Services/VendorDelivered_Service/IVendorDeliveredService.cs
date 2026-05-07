using CRM.Application.Services.Order_Service;

namespace CRM.Application.Services.VendorDelivered_Service
{
    public interface IVendorDeliveredService
    {
        Task<UpdateOrderVendorForwardStatusResultViewModel> MarkVendorDeliveredAsync(
            long orderId,
            long vendorId,
            string? actorUserId,
            string actorName,
            bool isVendorActor,
            CancellationToken ct);

        Task<List<VendorDeliveredListItemViewModel>> GetListAsync(string? userId, CancellationToken ct);
        Task<VendorDeliveredViewModel?> GetByOrderVendorAsync(long orderId, long vendorId, CancellationToken ct);
        Task<VendorDeliveredViewModel?> UpdateAsync(FinalizeVendorDeliveredViewModel model, CancellationToken ct);
        Task<VendorDeliveredViewModel?> UpdateShipmentAsync(UpdateVendorDeliveredShipmentViewModel model, CancellationToken ct);
        Task<VendorDeliveredViewModel?> FinalizeAsync(FinalizeVendorDeliveredViewModel model, CancellationToken ct);
    }
}
