using CRM.Application.Common.Pagination;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Order_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace CRM.Application.Services.CustomerDelivered_Service
{
    public class CustomerDeliveredService : ICustomerDeliveredService
    {
        private const string ForwardStatusDelivered = "delivered";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        public CustomerDeliveredService(IUnitOfWork unitOfWork, IWorkContext workContext)
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
            var existingDelivery = await LoadDeliveryByOrderAsync(orderId, ct);
            var alreadyExists = existingDelivery != null;
            if (existingDelivery == null)
            {
                existingDelivery = await CreateDraftAsync(orderId, actorName, now, ct);
                if (existingDelivery == null)
                {
                    await transaction.RollbackAsync(ct);
                    return new UpdateOrderVendorForwardStatusResultViewModel
                    {
                        Success = false,
                        Message = "No vendor delivery records were available to generate the final customer delivery draft."
                    };
                }
            }
            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new UpdateOrderVendorForwardStatusResultViewModel
            {
                Success = true,
                Message = existingDelivery.IsFinalized
                    ? "Customer delivery already finalized for this order."
                    : alreadyExists
                        ? "Existing merged customer delivery draft loaded."
                        : "Merged customer delivery draft created successfully.",
                RequiresFinalization = !existingDelivery.IsFinalized,
                IsLocked = true,
                AlreadyExists = alreadyExists,
                CustomerDelivered = await MapToViewModelAsync(existingDelivery, ct)
            };
        }
        public async Task<PaginatedResult<CustomerDeliveredListItemViewModel>> GetListAsync(
            PaginationRequest request,
            string? shipmentStatus,
            bool? isFinalized,
            string? userId,
            CancellationToken ct)
        {
            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? userId;
            var customer = await ResolveCustomerUserAsync(resolvedUserId, ct);
            var safePageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var safePageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            IQueryable<CustomerDelivered> query = _unitOfWork.CustomerDelivereds.Query()
                .Where(item => item.IsDelete == 0)
                .Include(item => item.Order)
                .Include(item => item.Details);
            if (customer != null)
                query = query.Where(item => item.CustomerId == customer.Id);
            if (!string.IsNullOrWhiteSpace(shipmentStatus))
            {
                var normalizedShipmentStatus = shipmentStatus.Trim().ToLowerInvariant();
                query = query.Where(item => item.ShipmentStatus.ToLower() == normalizedShipmentStatus);
            }
            if (isFinalized.HasValue)
                query = query.Where(item => item.IsFinalized == isFinalized.Value);
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(item =>
                    (item.Order != null && (
                        (item.Order.OrderNumber != null && item.Order.OrderNumber.ToLower().Contains(searchTerm)) ||
                        (((item.Order.FirstName ?? string.Empty) + " " + (item.Order.LastName ?? string.Empty)).Trim().ToLower().Contains(searchTerm)) ||
                        (item.Order.Phone != null && item.Order.Phone.ToLower().Contains(searchTerm))
                    )) ||
                    item.ShipmentStatus.ToLower().Contains(searchTerm) ||
                    (item.ShipmentProvider != null && item.ShipmentProvider.ToLower().Contains(searchTerm)) ||
                    (item.TrackingNumber != null && item.TrackingNumber.ToLower().Contains(searchTerm))
                );
            }
            var totalRecords = await query.CountAsync(ct);
            var deliveries = await query
                .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync(ct);
            var productLookup = await BuildProductLookupAsync(
                deliveries.SelectMany(item => item.Details).Select(item => item.ProductId),
                ct);
            var vendorLookup = await BuildVendorLookupAsync(
                deliveries.SelectMany(item => item.Details).Select(item => item.VendorId),
                ct);
            var items = deliveries
                .Select(item => MapToListItemViewModel(item, productLookup, vendorLookup))
                .ToList();
            var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)safePageSize);
            return new PaginatedResult<CustomerDeliveredListItemViewModel>
            {
                Items = items,
                TotalCount = totalRecords,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                TotalPages = totalPages,
                HasNextPage = safePageNumber < totalPages,
                HasPreviousPage = safePageNumber > 1
            };
        }
        public async Task<CustomerDeliveredViewModel?> GetByOrderAsync(long orderId, CancellationToken ct)
        {
            var delivery = await LoadDeliveryByOrderAsync(orderId, ct);
            if (delivery != null)
                return await MapToViewModelAsync(delivery, ct);
            var currentUser = await _workContext.CurrentUserAsync();
            var actorName = currentUser?.FullName ?? "Agora Team";
            var now = DateTime.UtcNow;
            delivery = await CreateDraftAsync(orderId, actorName, now, ct);
            if (delivery == null)
                return null;
            await _unitOfWork.SaveChangesAsync(ct);
            return await MapToViewModelAsync(delivery, ct);
        }
        public Task<CustomerDeliveredViewModel?> GetByOrderVendorAsync(long orderId, long vendorId, CancellationToken ct)
        {
            return GetByOrderAsync(orderId, ct);
        }
        public async Task<CustomerDeliveredViewModel?> FinalizeAsync(FinalizeCustomerDeliveredViewModel model, CancellationToken ct)
        {
            var delivery = await _unitOfWork.CustomerDelivereds.Query()
                .Where(item => item.Id == model.Id && item.IsDelete == 0)
                .Include(item => item.Order)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .FirstOrDefaultAsync(ct);
            if (delivery == null || delivery.IsFinalized)
                return null;
            var currentUser = await _workContext.CurrentUserAsync();
            var actorName = currentUser?.FullName ?? "Agora Team";
            var now = DateTime.UtcNow;
            delivery.ShipmentStatus = NormalizeShipmentStatus(model.ShipmentStatus);
            delivery.ShipmentProvider = NormalizeNullable(model.ShipmentProvider);
            delivery.TrackingNumber = NormalizeNullable(model.TrackingNumber);
            delivery.ShipmentInfo = NormalizeNullable(model.ShipmentInfo);
            delivery.DiscountAmount = model.DiscountAmount;
            delivery.ShipmentCharge = model.ShipmentCharge;
            delivery.VatAmount = model.VatAmount;
            delivery.IsFinalized = true;
            delivery.UpdatedAt = now;
            delivery.UpdatedBy = actorName;
            _unitOfWork.CustomerDelivereds.Update(delivery);
            await _unitOfWork.SaveChangesAsync(ct);
            return await MapToViewModelAsync(delivery, ct);
        }
        private Task<CustomerDelivered?> LoadDeliveryByOrderAsync(long orderId, CancellationToken ct)
        {
            return _unitOfWork.CustomerDelivereds.Query()
                .Where(item => item.OrderId == orderId && item.IsDelete == 0)
                .Include(item => item.Order)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .OrderBy(item => item.Id)
                .FirstOrDefaultAsync(ct);
        }
        private async Task<CustomerDelivered?> CreateDraftAsync(long orderId, string actorName, DateTime now, CancellationToken ct)
        {
            var vendorDeliveries = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.OrderId == orderId && item.IsDelete == 0)
                .Include(item => item.Order)
                .Include(item => item.Vendor)
                .Include(item => item.Details.OrderBy(detail => detail.Id))
                .OrderBy(item => item.Id)
                .ToListAsync(ct);
            if (!vendorDeliveries.Any())
                return null;
            var order = vendorDeliveries[0].Order;
            var allDetails = vendorDeliveries
                .SelectMany(item => item.Details
                    .Where(detail => detail.IsDelete == 0 && detail.ProductId > 0 && detail.Quantity > 0)
                    .Select(detail => new CustomerDeliveredDetail
                    {
                        ProductId = detail.ProductId,
                        VendorId = item.VendorId,
                        VendorDeliveredId = item.Id,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        CreatedAt = now,
                        CreatedBy = actorName,
                        IsDelete = 0
                    }))
                .ToList();
            if (!allDetails.Any())
                return null;
            var delivery = new CustomerDelivered
            {
                OrderId = orderId,
                CustomerId = order?.CustomerId,
                SubTotal = vendorDeliveries.Sum(item => item.SubTotal),
                DiscountAmount = vendorDeliveries.Sum(item => item.DiscountAmount),
                ShipmentCharge = vendorDeliveries.Sum(item => item.ShipmentCharge),
                VatAmount = vendorDeliveries.Sum(item => item.VatAmount),
                ShipmentStatus = "Pending",
                ShipmentProvider = null,
                TrackingNumber = null,
                ShipmentInfo = null,
                IsFinalized = false,
                CreatedAt = now,
                CreatedBy = actorName,
                IsDelete = 0,
                Order = order,
                Details = allDetails
            };
            await _unitOfWork.CustomerDelivereds.AddAsync(delivery, ct);
            return delivery;
        }
        private async Task<CustomerDeliveredViewModel> MapToViewModelAsync(CustomerDelivered delivery, CancellationToken ct)
        {
            var productLookup = await BuildProductLookupAsync(
                delivery.Details.Select(item => item.ProductId),
                ct);
            var vendorLookup = await BuildVendorLookupAsync(
                delivery.Details.Select(item => item.VendorId),
                ct);
            return MapToViewModel(delivery, productLookup, vendorLookup);
        }
        private static CustomerDeliveredViewModel MapToViewModel(
            CustomerDelivered delivery,
            IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> productLookup,
            IReadOnlyDictionary<long, (string? VendorName, string? VendorCompanyName)> vendorLookup)
        {
            static string? GetProductName(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductName : null;
            static string? GetProductCode(
                IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> lookup,
                long productId) =>
                lookup.TryGetValue(productId, out var productInfo) ? productInfo.ProductCode : null;
            static string? GetVendorName(
                IReadOnlyDictionary<long, (string? VendorName, string? VendorCompanyName)> lookup,
                long? vendorId) =>
                vendorId.HasValue && lookup.TryGetValue(vendorId.Value, out var vendorInfo) ? vendorInfo.VendorName : null;
            static string? GetVendorCompanyName(
                IReadOnlyDictionary<long, (string? VendorName, string? VendorCompanyName)> lookup,
                long? vendorId) =>
                vendorId.HasValue && lookup.TryGetValue(vendorId.Value, out var vendorInfo) ? vendorInfo.VendorCompanyName : null;
            var customerName = delivery.Order == null
                ? null
                : string.Join(" ", new[] { delivery.Order.FirstName, delivery.Order.LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)))
                    .Trim();
            return new CustomerDeliveredViewModel
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                CustomerId = delivery.CustomerId,
                OrderNumber = delivery.Order?.OrderNumber,
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
                ShipmentInfo = delivery.ShipmentInfo,
                IsFinalized = delivery.IsFinalized,
                CreatedAt = delivery.CreatedAt,
                UpdatedAt = delivery.UpdatedAt,
                Details = delivery.Details
                    .Where(item => item.IsDelete == 0)
                    .OrderBy(item => item.Id)
                    .Select(item => new CustomerDeliveredDetailViewModel
                    {
                        Id = item.Id,
                        CustomerDeliveredId = item.CustomerDeliveredId,
                        ProductId = item.ProductId,
                        VendorId = item.VendorId,
                        VendorDeliveredId = item.VendorDeliveredId,
                        VendorName = GetVendorName(vendorLookup, item.VendorId),
                        VendorCompanyName = GetVendorCompanyName(vendorLookup, item.VendorId),
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice,
                        ProductName = GetProductName(productLookup, item.ProductId),
                        ProductCode = GetProductCode(productLookup, item.ProductId)
                    })
                    .ToList()
            };
        }
        private static CustomerDeliveredListItemViewModel MapToListItemViewModel(
            CustomerDelivered delivery,
            IReadOnlyDictionary<long, (string? ProductName, string? ProductCode)> productLookup,
            IReadOnlyDictionary<long, (string? VendorName, string? VendorCompanyName)> vendorLookup)
        {
            var mapped = MapToViewModel(delivery, productLookup, vendorLookup);
            return new CustomerDeliveredListItemViewModel
            {
                Id = mapped.Id,
                OrderId = mapped.OrderId,
                CustomerId = mapped.CustomerId,
                OrderNumber = mapped.OrderNumber,
                CustomerName = mapped.CustomerName,
                CustomerPhone = mapped.CustomerPhone,
                OrderDate = mapped.OrderDate,
                SubTotal = mapped.SubTotal,
                DiscountAmount = mapped.DiscountAmount,
                ShipmentCharge = mapped.ShipmentCharge,
                VatAmount = mapped.VatAmount,
                TotalAmount = mapped.TotalAmount,
                ShipmentStatus = mapped.ShipmentStatus,
                ShipmentProvider = mapped.ShipmentProvider,
                TrackingNumber = mapped.TrackingNumber,
                ShipmentInfo = mapped.ShipmentInfo,
                IsFinalized = mapped.IsFinalized,
                CreatedAt = mapped.CreatedAt,
                UpdatedAt = mapped.UpdatedAt,
                Details = mapped.Details,
                TotalItems = mapped.Details.Count,
                TotalQuantity = mapped.Details.Sum(item => item.Quantity)
            };
        }
        private async Task<Dictionary<long, (string? ProductName, string? ProductCode)>> BuildProductLookupAsync(
            IEnumerable<long> productIds,
            CancellationToken ct)
        {
            var ids = productIds.Where(id => id > 0).Distinct().ToList();
            if (!ids.Any())
                return new Dictionary<long, (string? ProductName, string? ProductCode)>();
            return await _unitOfWork.Products.Query()
                .Where(item => item.IsDelete == 0 && ids.Contains(item.Id))
                .Select(item => new { item.Id, item.ProductName, item.ProductCode })
                .ToDictionaryAsync(
                    item => item.Id,
                    item => (item.ProductName, item.ProductCode),
                    ct);
        }
        private async Task<Dictionary<long, (string? VendorName, string? VendorCompanyName)>> BuildVendorLookupAsync(
            IEnumerable<long?> vendorIds,
            CancellationToken ct)
        {
            var ids = vendorIds
                .Where(id => id.HasValue && id.Value > 0)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            if (!ids.Any())
                return new Dictionary<long, (string? VendorName, string? VendorCompanyName)>();
            return await _unitOfWork.Vendors.Query()
                .Where(item => item.IsDelete == 0 && ids.Contains(item.Id))
                .Select(item => new { item.Id, item.Name, item.CompanyName })
                .ToDictionaryAsync(
                    item => item.Id,
                    item => (item.Name, item.CompanyName),
                    ct);
        }
        private async Task<Customer?> ResolveCustomerUserAsync(string? userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;
            return await _unitOfWork.Customers.Query()
                .Where(customer => customer.IsDelete == 0 && customer.IsActive && customer.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }
        private static string NormalizeShipmentStatus(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Pending" : value.Trim();
        }
        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
    }
}
