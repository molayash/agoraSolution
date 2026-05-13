using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Order_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.VendorDelivered_Service
{
    public class VendorDeliveredService : IVendorDeliveredService
    {
        private const string ForwardStatusDelivered = "delivered";
        private const string ForwardStatusLegacyDelivery = "delivery";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;

        public VendorDeliveredService(IUnitOfWork unitOfWork, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
        }

        public async Task<UpdateOrderVendorForwardStatusResultViewModel> MarkVendorDeliveredAsync(
            long orderId,
            long vendorId,
            string? actorUserId,
            string actorName,
            bool isVendorActor,
            CancellationToken ct)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

            var latestForward = await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward =>
                    forward.OrderId == orderId &&
                    forward.VendorId == vendorId &&
                    forward.IsDelete == 0 &&
                    forward.IsSuccess)
                .OrderByDescending(forward => forward.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestForward == null)
            {
                await transaction.RollbackAsync(ct);
                return new UpdateOrderVendorForwardStatusResultViewModel
                {
                    Success = false,
                    Message = "Vendor forward thread was not found."
                };
            }

            var existingDelivery = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.OrderId == orderId && item.VendorId == vendorId && item.IsDelete == 0)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);

            var now = DateTime.UtcNow;

            latestForward.FulfillmentStatus = ForwardStatusDelivered;
            latestForward.StatusUpdatedAt = now;
            latestForward.StatusUpdatedByUserId = actorUserId;
            latestForward.StatusUpdatedByName = actorName;
            latestForward.IsLocked = true;
            latestForward.UpdatedAt = now;
            latestForward.UpdatedBy = actorName;

            ApplyViewStateAfterActorActivity(latestForward, isVendorActor, now);
            _unitOfWork.OrderVendorForwards.Update(latestForward);

            var alreadyExists = existingDelivery != null;
            if (existingDelivery == null)
            {
                existingDelivery = await CreateDraftAsync(orderId, vendorId, actorName, now, ct);
                if (existingDelivery == null)
                {
                    await transaction.RollbackAsync(ct);
                    return new UpdateOrderVendorForwardStatusResultViewModel
                    {
                        Success = false,
                        Message = "No vendor-owned order items were available to generate the delivery draft."
                    };
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return new UpdateOrderVendorForwardStatusResultViewModel
            {
                Success = true,
                Message = existingDelivery.IsFinalized
                    ? "Delivery already finalized for this vendor."
                    : alreadyExists
                        ? "Existing delivery draft loaded for finalization."
                        : "Delivery draft created successfully.",
                RequiresFinalization = !existingDelivery.IsFinalized,
                IsLocked = true,
                AlreadyExists = alreadyExists,
                VendorDelivered = await MapToViewModelAsync(existingDelivery, ct)
            };
        }

        public async Task<List<VendorDeliveredListItemViewModel>> GetListAsync(string? userId, CancellationToken ct)
        {
            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? userId;
            var vendor = await ResolveVendorUserAsync(resolvedUserId, ct);

            IQueryable<VendorDelivered> query = _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.IsDelete == 0)
                .Include(item => item.Order)
                .Include(item => item.Vendor)
                .Include(item => item.Details);

            if (vendor != null)
                query = query.Where(item => item.VendorId == vendor.Id);

            var deliveries = await query
                .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                .ToListAsync(ct);

            var productLookup = await BuildProductLookupAsync(
                deliveries.SelectMany(item => item.Details).Select(item => item.ProductId),
                ct);

            return deliveries
                .Select(item => MapToListItemViewModel(item, productLookup))
                .ToList();
        }

        public async Task<VendorDeliveredViewModel?> GetByOrderVendorAsync(long orderId, long vendorId, CancellationToken ct)
        {
            var delivery = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.OrderId == orderId && item.VendorId == vendorId && item.IsDelete == 0)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);

            return delivery == null ? null : await MapToViewModelAsync(delivery, ct);
        }

        public async Task<VendorDeliveredViewModel?> UpdateAsync(FinalizeVendorDeliveredViewModel model, CancellationToken ct)
        {
            var delivery = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.Id == model.Id && item.IsDelete == 0)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);

            if (delivery == null)
                return null;

            var currentUser = await _workContext.CurrentUserAsync();
            var actorName = currentUser?.FullName ?? "Agora Team";
            var now = DateTime.UtcNow;

            delivery.ShipmentStatus = NormalizeShipmentStatus(model.ShipmentStatus);
            delivery.ShipmentProvider = NormalizeNullable(model.ShipmentProvider);
            delivery.TrackingNumber = NormalizeNullable(model.TrackingNumber);
            delivery.ShipmentLiveTrackLink = NormalizeNullable(model.ShipmentLiveTrackLink);
            delivery.ShipmentInfo = NormalizeNullable(model.ShipmentInfo);
            delivery.DiscountAmount = model.DiscountAmount;
            delivery.ShipmentCharge = model.ShipmentCharge;
            delivery.VatAmount = model.VatAmount;
            delivery.UpdatedAt = now;
            delivery.UpdatedBy = actorName;

            _unitOfWork.VendorDelivereds.Update(delivery);
            await _unitOfWork.SaveChangesAsync(ct);

            return await MapToViewModelAsync(delivery, ct);
        }

        public async Task<VendorDeliveredViewModel?> UpdateShipmentAsync(UpdateVendorDeliveredShipmentViewModel model, CancellationToken ct)
        {
            var delivery = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.Id == model.Id && item.IsDelete == 0)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);

            if (delivery == null || !delivery.IsFinalized)
                return null;

            var currentUser = await _workContext.CurrentUserAsync();
            var actorName = currentUser?.FullName ?? "Agora Team";
            var now = DateTime.UtcNow;

            delivery.ShipmentStatus = NormalizeShipmentStatus(model.ShipmentStatus);
            delivery.ShipmentProvider = NormalizeNullable(model.ShipmentProvider);
            delivery.TrackingNumber = NormalizeNullable(model.TrackingNumber);
            delivery.ShipmentLiveTrackLink = NormalizeNullable(model.ShipmentLiveTrackLink);
            delivery.ShipmentInfo = NormalizeNullable(model.ShipmentInfo);
            delivery.UpdatedAt = now;
            delivery.UpdatedBy = actorName;

            _unitOfWork.VendorDelivereds.Update(delivery);
            await _unitOfWork.SaveChangesAsync(ct);

            return await MapToViewModelAsync(delivery, ct);
        }

        public async Task<VendorDeliveredViewModel?> FinalizeAsync(FinalizeVendorDeliveredViewModel model, CancellationToken ct)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

            var delivery = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.Id == model.Id && item.IsDelete == 0)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);

            if (delivery == null || delivery.IsFinalized)
            {
                await transaction.RollbackAsync(ct);
                return null;
            }

            var currentUser = await _workContext.CurrentUserAsync();
            var actorName = currentUser?.FullName ?? "Agora Team";
            var now = DateTime.UtcNow;
            var activeDetails = delivery.Details
                .Where(item => item.IsDelete == 0 && item.ProductId > 0 && item.Quantity > 0)
                .ToList();

            if (activeDetails.Count > 0)
            {
                var quantitiesByProductId = activeDetails
                    .GroupBy(item => item.ProductId)
                    .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Quantity));

                var productIds = quantitiesByProductId.Keys.ToList();
                var products = await _unitOfWork.Products.Query()
                    .Where(product => product.IsDelete == 0 && productIds.Contains(product.Id))
                    .ToListAsync(ct);

                foreach (var product in products)
                {
                    if (!quantitiesByProductId.TryGetValue(product.Id, out var deliveredQuantity))
                        continue;

                    product.StockItems = Math.Max(0, product.StockItems - deliveredQuantity);
                    product.UpdatedAt = now;
                    product.UpdatedBy = actorName;
                    _unitOfWork.Products.Update(product);
                }
            }

            delivery.ShipmentStatus = NormalizeShipmentStatus(model.ShipmentStatus);
            delivery.ShipmentProvider = NormalizeNullable(model.ShipmentProvider);
            delivery.TrackingNumber = NormalizeNullable(model.TrackingNumber);
            delivery.ShipmentLiveTrackLink = NormalizeNullable(model.ShipmentLiveTrackLink);
            delivery.ShipmentInfo = NormalizeNullable(model.ShipmentInfo);
            delivery.DiscountAmount = model.DiscountAmount;
            delivery.ShipmentCharge = model.ShipmentCharge;
            delivery.VatAmount = model.VatAmount;
            delivery.IsFinalized = true;
            delivery.UpdatedAt = now;
            delivery.UpdatedBy = actorName;

            _unitOfWork.VendorDelivereds.Update(delivery);
            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return await MapToViewModelAsync(delivery, ct);
        }

        private async Task<VendorDelivered?> CreateDraftAsync(long orderId, long vendorId, string actorName, DateTime now, CancellationToken ct)
        {
            var order = await _unitOfWork.Orders.Query()
                .Where(item => item.Id == orderId && item.IsDelete == 0)
                .Include(item => item.OrderItems)
                .FirstOrDefaultAsync(ct);

            if (order == null)
                return null;

            var productIds = order.OrderItems.Select(item => item.ProductId).Distinct().ToList();
            if (productIds.Count == 0)
                return null;

            var vendorProductIds = await _unitOfWork.Products.Query()
                .Where(product => product.IsDelete == 0 && product.VendorId == vendorId && productIds.Contains(product.Id))
                .Select(product => product.Id)
                .ToListAsync(ct);

            var vendorItems = order.OrderItems
                .Where(item => vendorProductIds.Contains(item.ProductId))
                .ToList();

            if (vendorItems.Count == 0)
                return null;

            var delivery = new VendorDelivered
            {
                VendorDeliveredStringId = GenerateVendorDeliveredStringId(),
                OrderId = orderId,
                VendorId = vendorId,
                SubTotal = vendorItems.Sum(item => item.UnitPrice * item.Quantity),
                DiscountAmount = 0,
                ShipmentCharge = 0,
                VatAmount = 0,
                ShipmentStatus = "Pending",
                ShipmentProvider = null,
                TrackingNumber = null,
                ShipmentLiveTrackLink = null,
                ShipmentInfo = null,
                IsFinalized = false,
                CreatedAt = now,
                CreatedBy = actorName,
                IsDelete = 0,
                Details = vendorItems.Select(item => new VendorDeliveredDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CreatedAt = now,
                    CreatedBy = actorName,
                    IsDelete = 0
                }).ToList()
            };

            await _unitOfWork.VendorDelivereds.AddAsync(delivery, ct);
            return delivery;
        }

        private async Task<VendorDeliveredViewModel> MapToViewModelAsync(VendorDelivered delivery, CancellationToken ct)
        {
            var productLookup = await BuildProductLookupAsync(
                delivery.Details.Select(item => item.ProductId),
                ct);

            return MapToViewModel(delivery, productLookup);
        }

        private static VendorDeliveredViewModel MapToViewModel(
            VendorDelivered delivery,
            IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> productLookup)
        {
            static string? GetProductName(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductName : null;

            static string? GetProductCode(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductCode : null;

            return new VendorDeliveredViewModel
            {
                Id = delivery.Id,
                VendorDeliveredStringId = delivery.VendorDeliveredStringId,
                OrderId = delivery.OrderId,
                VendorId = delivery.VendorId,
                OrderNumber = delivery.Order?.OrderNumber,
                VendorName = delivery.Vendor?.Name,
                VendorCompanyName = delivery.Vendor?.CompanyName,
                CustomerName = delivery.Order == null ? null : string.Join(" ", new[] { delivery.Order.FirstName, delivery.Order.LastName }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim(),
                CustomerPhone = delivery.Order?.Phone,
                OrderDate = delivery.Order?.OrderDate,
                SubTotal = delivery.SubTotal,
                DiscountAmount = delivery.DiscountAmount,
                ShipmentCharge = delivery.ShipmentCharge,
                VatAmount = delivery.VatAmount,
                TotalAmount = delivery.SubTotal - delivery.DiscountAmount + delivery.ShipmentCharge + delivery.VatAmount,
                ShipmentStatus = NormalizeShipmentStatus(delivery.ShipmentStatus),
                ShipmentProvider = delivery.ShipmentProvider,
                TrackingNumber = delivery.TrackingNumber,
                ShipmentLiveTrackLink = delivery.ShipmentLiveTrackLink,
                ShipmentInfo = delivery.ShipmentInfo,
                IsFinalized = delivery.IsFinalized,
                CreatedAt = delivery.CreatedAt,
                UpdatedAt = delivery.UpdatedAt,
                Details = delivery.Details
                    .Where(item => item.IsDelete == 0)
                    .OrderBy(item => item.Id)
                    .Select(item => new VendorDeliveredDetailViewModel
                    {
                        Id = item.Id,
                        VendorDeliveredId = item.VendorDeliveredId,
                        ProductId = item.ProductId,
                        ProductName = GetProductName(productLookup, item.ProductId),
                        ProductCode = GetProductCode(productLookup, item.ProductId),
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    })
                    .ToList()
            };
        }

        private static VendorDeliveredListItemViewModel MapToListItemViewModel(
            VendorDelivered delivery,
            IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> productLookup)
        {
            static string? GetProductName(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductName : null;

            static string? GetProductCode(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductCode : null;

            var details = delivery.Details
                .Where(item => item.IsDelete == 0)
                .OrderBy(item => item.Id)
                .ToList();

            var customerName = delivery.Order == null
                ? null
                : string.Join(" ", new[] { delivery.Order.FirstName, delivery.Order.LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)))
                    .Trim();

            return new VendorDeliveredListItemViewModel
            {
                Id = delivery.Id,
                VendorDeliveredStringId = delivery.VendorDeliveredStringId,
                OrderId = delivery.OrderId,
                VendorId = delivery.VendorId,
                OrderNumber = delivery.Order?.OrderNumber,
                VendorName = delivery.Vendor?.Name,
                VendorCompanyName = delivery.Vendor?.CompanyName,
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? null : customerName,
                CustomerPhone = delivery.Order?.Phone,
                OrderDate = delivery.Order?.OrderDate,
                SubTotal = delivery.SubTotal,
                DiscountAmount = delivery.DiscountAmount,
                ShipmentCharge = delivery.ShipmentCharge,
                VatAmount = delivery.VatAmount,
                TotalAmount = delivery.SubTotal - delivery.DiscountAmount + delivery.ShipmentCharge + delivery.VatAmount,
                ShipmentStatus = NormalizeShipmentStatus(delivery.ShipmentStatus),
                ShipmentProvider = delivery.ShipmentProvider,
                TrackingNumber = delivery.TrackingNumber,
                ShipmentLiveTrackLink = delivery.ShipmentLiveTrackLink,
                ShipmentInfo = delivery.ShipmentInfo,
                IsFinalized = delivery.IsFinalized,
                CreatedAt = delivery.CreatedAt,
                UpdatedAt = delivery.UpdatedAt,
                TotalItems = details.Count,
                TotalQuantity = details.Sum(item => item.Quantity),
                Details = details.Select(item => new VendorDeliveredDetailViewModel
                {
                    Id = item.Id,
                    VendorDeliveredId = item.VendorDeliveredId,
                    ProductId = item.ProductId,
                    ProductName = GetProductName(productLookup, item.ProductId),
                    ProductCode = GetProductCode(productLookup, item.ProductId),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList()
            };
        }

        private async Task<Dictionary<long, (string? ProductName, string? ProductCode)>> BuildProductLookupAsync(
            IEnumerable<long> productIds,
            CancellationToken ct)
        {
            var ids = productIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<long, (string? ProductName, string? ProductCode)>();

            return await _unitOfWork.Products.Query()
                .Where(product => product.IsDelete == 0 && ids.Contains(product.Id))
                .Select(product => new
                {
                    product.Id,
                    product.ProductName,
                    product.ProductCode
                })
                .ToDictionaryAsync(
                    item => item.Id,
                    item => ((string?)item.ProductName, (string?)item.ProductCode),
                    ct);
        }

        private static string NormalizeShipmentStatus(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Pending" : value.Trim();
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string GenerateVendorDeliveredStringId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Random.Shared.Next(1000, 9999);
            return $"VDL-{timestamp}-{random}";
        }

        private static void ApplyViewStateAfterActorActivity(OrderVendorForward forward, bool isVendorActor, DateTime timestamp)
        {
            if (isVendorActor)
            {
                forward.VendorLastViewedAt = timestamp;
                forward.AdminLastViewedAt = null;
                return;
            }

            forward.AdminLastViewedAt = timestamp;
            forward.VendorLastViewedAt = null;
        }

        private async Task<Vendor?> ResolveVendorUserAsync(string? userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _unitOfWork.Vendors.Query()
                .Where(vendor => vendor.IsDelete == 0 && vendor.IsActive && vendor.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }
    }
}
