using CRM.Application.Common.Pagination;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.VendorDelivered_Service;
using CRM.Application.Services.Email_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CRM.Application.Services.Order_Service
{
    public class OrderService : IOrderService
    {
        private const string ForwardStatusPending = "pending";
        private const string ForwardStatusAccepted = "accepted";
        private const string ForwardStatusProcessing = "processing";
        private const string ForwardStatusDelivered = "delivered";
        private const string ForwardStatusLegacyDelivery = "delivery";
        private const string ForwardStatusCancelled = "cancelled";

        private const string ViewerRoleAdmin = "admin";
        private const string ViewerRoleVendor = "vendor";
        private const string ViewerRoleUnknown = "unknown";

        private const string SenderRoleAdmin = "admin";
        private const string SenderRoleVendor = "vendor";

        private static readonly string[] AllowedForwardStatuses =
        {
            ForwardStatusPending,
            ForwardStatusAccepted,
            ForwardStatusProcessing,
            ForwardStatusDelivered,
            ForwardStatusCancelled,
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IWorkContext _workContext;
        private readonly IVendorDeliveredService _vendorDeliveredService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IWorkContext workContext,
            IVendorDeliveredService vendorDeliveredService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _workContext = workContext;
            _vendorDeliveredService = vendorDeliveredService;
        }

        public async Task<int> CreateOrder(OrderViewModel model, CancellationToken ct)
        {
            try
            {
                var currentUser = await _workContext.CurrentUserAsync();
                var customer = await ResolveCustomerUserAsync(currentUser?.Id, ct);
                var customerEmail = customer?.Email;

                if (string.IsNullOrWhiteSpace(customerEmail) && !string.IsNullOrWhiteSpace(model.CustomerEmail))
                    customerEmail = model.CustomerEmail.Trim();

                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    CustomerId = customer?.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    Phone = model.Phone,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    SubTotal = model.SubTotal,
                    ShippingFee = model.ShippingFee,
                    Tax = model.Tax,
                    TotalAmount = model.TotalAmount,
                    Status = "pending",
                    CustomerQuery = model.CustomerQuery,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsDelete = 0
                };

                foreach (var item in model.Items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Name = item.Name,
                        ImageUrl = item.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        IsDelete = 0
                    });
                }

                await _unitOfWork.Orders.AddAsync(order, ct);
                var result = await _unitOfWork.SaveChangesAsync(ct);

                if (result > 0)
                {
                    var customerOrderSnapshot = BuildCustomerOrderEmailSnapshot(order, customerEmail);
                    _ = Task.Run(() => SendCustomerOrderConfirmationInBackgroundAsync(customerOrderSnapshot));
                }

                return (int)(result > 0 ? order.Id : 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                return 0;
            }
        }

        public async Task<OrderViewModel> GetAllOrders(CancellationToken ct)
        {
            var orders = await LoadOrdersAsync(
                _unitOfWork.Orders.Query()
                    .Where(order => order.IsDelete == 0)
                    .OrderByDescending(order => order.OrderDate),
                null,
                ct);

            return new OrderViewModel
            {
                OrderList = orders.AsQueryable()
            };
        }

        public async Task<OrderViewModel> GetOrderById(long id, CancellationToken ct)
        {
            var order = await _unitOfWork.Orders.Query()
                .Where(item => item.Id == id && item.IsDelete == 0)
                .Include(item => item.OrderItems)
                .FirstOrDefaultAsync(ct);

            if (order == null)
                return null;

            var currentUser = await _workContext.CurrentUserAsync();
            var vendorViewer = await ResolveVendorUserAsync(currentUser?.Id, ct);

            if (vendorViewer != null)
            {
                var hasAccess = await _unitOfWork.OrderVendorForwards.Query()
                    .AnyAsync(forward =>
                        forward.OrderId == id &&
                        forward.VendorId == vendorViewer.Id &&
                        forward.IsDelete == 0 &&
                        forward.IsSuccess,
                        ct);

                if (!hasAccess)
                    return null;
            }

            return await BuildOrderViewModelAsync(order, vendorViewer?.Id, ct);
        }

        public async Task<int> UpdateOrderStatus(UpdateOrderStatusViewModel model, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.Orders.Query()
                    .Where(item => item.Id == model.Id && item.IsDelete == 0)
                    .Include(item => item.OrderItems)
                    .FirstOrDefaultAsync(ct);

                if (order == null)
                    return 1;

                var normalizedStatus = model.Status.Trim().ToLowerInvariant();
                order.Status = normalizedStatus;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.SaveChangesAsync(ct);

                if (result > 0 && normalizedStatus == ForwardStatusProcessing)
                    await AutoForwardOrderToVendorsAsync(order, ct);

                return result > 0 ? 2 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> UpdateCustomerQuery(UpdateCustomerQueryViewModel model, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.Orders.Query()
                    .FirstOrDefaultAsync(item => item.Id == model.Id && item.IsDelete == 0, ct);

                if (order == null)
                    return false;

                var currentUser = await _workContext.CurrentUserAsync();
                var customer = await ResolveCustomerUserAsync(currentUser?.Id, ct);
                if (customer != null && order.CustomerId != customer.Id)
                    return false;

                order.CustomerQuery = string.IsNullOrWhiteSpace(model.CustomerQuery)
                    ? null
                    : model.CustomerQuery.Trim();
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                return await _unitOfWork.SaveChangesAsync(ct) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating customer query: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteOrder(long id, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.Orders.Query()
                    .FirstOrDefaultAsync(item => item.Id == id && item.IsDelete == 0, ct);

                if (order == null)
                    return false;

                order.IsDelete = 1;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                return await _unitOfWork.SaveChangesAsync(ct) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order: {ex.Message}");
                return false;
            }
        }

        public async Task<PaginatedResult<OrderViewModel>> GetOrdersPagination(PaginationRequest request, CancellationToken ct)
        {
            var query = _unitOfWork.Orders.Query()
                .Where(order => order.IsDelete == 0)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLowerInvariant();
                query = query.Where(order =>
                    order.OrderNumber.ToLower().Contains(searchTerm) ||
                    order.FirstName.ToLower().Contains(searchTerm) ||
                    order.LastName.ToLower().Contains(searchTerm) ||
                    order.Phone.Contains(searchTerm) ||
                    order.Status.ToLower().Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync(ct);

            var pageOrders = await query
                .OrderByDescending(order => order.OrderDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(order => order.OrderItems)
                .ToListAsync(ct);

            var items = new List<OrderViewModel>();
            foreach (var order in pageOrders)
            {
                var mapped = await BuildOrderViewModelAsync(order, null, ct);
                if (mapped != null)
                    items.Add(mapped);
            }

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new PaginatedResult<OrderViewModel>
            {
                Items = items,
                TotalCount = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.PageNumber < totalPages,
                HasPreviousPage = request.PageNumber > 1
            };
        }

        public async Task<List<OrderViewModel>> GetOrdersByCustomer(string phone, CancellationToken ct)
        {
            return await LoadOrdersAsync(
                _unitOfWork.Orders.Query()
                    .Where(order => order.Phone == phone && order.IsDelete == 0)
                    .OrderByDescending(order => order.OrderDate),
                null,
                ct);
        }

        public async Task<List<OrderViewModel>> GetOrdersByCustomerUserId(string userId, CancellationToken ct)
        {
            var customer = await ResolveCustomerUserAsync(userId, ct);
            if (customer == null)
                return new List<OrderViewModel>();

            return await LoadOrdersAsync(
                _unitOfWork.Orders.Query()
                    .Where(order => order.CustomerId == customer.Id && order.IsDelete == 0)
                    .OrderByDescending(order => order.OrderDate),
                null,
                ct);
        }

        public async Task<List<OrderViewModel>> GetOrdersByStatus(string status, CancellationToken ct)
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();

            return await LoadOrdersAsync(
                _unitOfWork.Orders.Query()
                    .Where(order => order.IsDelete == 0 && order.Status.ToLower() == normalizedStatus)
                    .OrderByDescending(order => order.OrderDate),
                null,
                ct);
        }

        public async Task<List<OrderViewModel>> GetMyOrders(string userId, CancellationToken ct)
        {
            var vendor = await ResolveVendorUserAsync(userId, ct);
            if (vendor == null)
                return new List<OrderViewModel>();

            var forwardedOrderIds = await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward => forward.IsDelete == 0 && forward.VendorId == vendor.Id && forward.IsSuccess)
                .OrderByDescending(forward => forward.CreatedAt)
                .Select(forward => forward.OrderId)
                .Distinct()
                .ToListAsync(ct);

            if (forwardedOrderIds.Count == 0)
                return new List<OrderViewModel>();

            var orders = await _unitOfWork.Orders.Query()
                .Where(order => order.IsDelete == 0 && forwardedOrderIds.Contains(order.Id))
                .Include(order => order.OrderItems)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync(ct);

            var vendorOrders = new List<OrderViewModel>();
            foreach (var order in orders)
            {
                var mapped = await BuildOrderViewModelAsync(order, vendor.Id, ct);
                if (mapped != null)
                    vendorOrders.Add(mapped);
            }

            return vendorOrders;
        }

        public async Task<bool> ForwardToVendor(ForwardOrderViewModel model, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.Orders.Query()
                    .Where(item => item.Id == model.OrderId && item.IsDelete == 0)
                    .Include(item => item.OrderItems)
                    .FirstOrDefaultAsync(ct);

                if (order == null)
                    return false;

                if (!long.TryParse(model.VendorId, out var vendorId))
                    return false;

                var vendor = await _unitOfWork.Vendors.Query()
                    .Where(item => item.Id == vendorId && item.IsDelete == 0 && item.IsActive)
                    .FirstOrDefaultAsync(ct);

                if (vendor == null)
                    return false;

                var currentUser = await _workContext.CurrentUserAsync();
                var fullOrder = await BuildOrderViewModelAsync(order, null, ct);
                if (fullOrder == null)
                    return false;

                var vendorOrder = FilterOrderForVendor(fullOrder, vendorId);
                if (vendorOrder == null)
                    return false;

                return await SendOrderToVendorAsync(
                    vendorOrder,
                    vendor,
                    model.Message,
                    currentUser?.Id ?? model.UserId,
                    currentUser?.FullName,
                    ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ForwardToVendor service: {ex.Message}");
                return false;
            }
        }

        public async Task<OrderVendorCommentResponseViewModel> GetForwardComments(long orderId, string? userId, bool markAsRead, CancellationToken ct)
        {
            var response = new OrderVendorCommentResponseViewModel { OrderId = orderId };

            var order = await _unitOfWork.Orders.Query()
                .Where(item => item.Id == orderId && item.IsDelete == 0)
                .FirstOrDefaultAsync(ct);

            if (order == null)
                return response;

            response.OrderNumber = order.OrderNumber;

            var viewer = await ResolveViewerContextAsync(userId, ct);
            var isVendorViewer = viewer.ViewerRole == ViewerRoleVendor;
            response.ViewerRole = viewer.ViewerRole;

            var forwards = await LoadSuccessfulForwardsAsync(orderId, viewer.Vendor?.Id, ct);
            if (!forwards.Any())
                return response;

            var vendorIds = forwards.Select(forward => forward.VendorId).Distinct().ToList();
            var comments = await LoadCommentsAsync(orderId, vendorIds, ct);

            if (markAsRead)
                await MarkThreadItemsAsReadAsync(forwards, comments, isVendorViewer, ct);

            response.Threads = forwards
                .GroupBy(forward => forward.VendorId)
                .Select(group => BuildThreadViewModel(group.OrderByDescending(item => item.CreatedAt).First(), comments, isVendorViewer))
                .OrderByDescending(thread => thread.LastCommentAt ?? thread.StatusUpdatedAt ?? thread.ForwardedAt)
                .ToList();

            return response;
        }

        public async Task<bool> AddForwardComment(CreateOrderVendorCommentViewModel model, CancellationToken ct)
        {
            var message = model.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return false;

            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? model.UserId;
            var vendorUser = await ResolveVendorUserAsync(resolvedUserId, ct);

            var targetVendorId = ResolveTargetVendorId(vendorUser, model.VendorId);
            if (!targetVendorId.HasValue)
                return false;

            var latestForward = await GetLatestForwardAsync(model.OrderId, targetVendorId.Value, ct);
            if (latestForward == null)
                return false;

            var now = DateTime.UtcNow;
            var isVendorSender = vendorUser != null;
            var actorName = currentUser?.FullName ?? vendorUser?.Name ?? "Agora Team";

            var comment = new OrderVendorComment
            {
                OrderId = model.OrderId,
                VendorId = targetVendorId.Value,
                OrderVendorForwardId = latestForward.Id,
                SenderUserId = resolvedUserId,
                SenderName = actorName,
                SenderRole = isVendorSender ? SenderRoleVendor : SenderRoleAdmin,
                Message = message,
                CreatedAt = now,
                CreatedBy = actorName,
                IsDelete = 0,
                AdminReadAt = isVendorSender ? null : now,
                VendorReadAt = isVendorSender ? now : null,
            };

            ApplyViewStateAfterActorActivity(latestForward, isVendorSender, now);
            latestForward.UpdatedAt = now;
            latestForward.UpdatedBy = actorName;

            _unitOfWork.OrderVendorForwards.Update(latestForward);
            await _unitOfWork.OrderVendorComments.AddAsync(comment, ct);

            return await _unitOfWork.SaveChangesAsync(ct) > 0;
        }

        public async Task<UpdateOrderVendorForwardStatusResultViewModel> UpdateForwardStatus(UpdateOrderVendorForwardStatusViewModel model, CancellationToken ct)
        {
            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? model.UserId;
            var vendorUser = await ResolveVendorUserAsync(resolvedUserId, ct);

            var targetVendorId = ResolveTargetVendorId(vendorUser, model.VendorId);
            if (!targetVendorId.HasValue)
            {
                return new UpdateOrderVendorForwardStatusResultViewModel
                {
                    Success = false,
                    Message = "Vendor access was not resolved for this order."
                };
            }

            var latestForward = await GetLatestForwardAsync(model.OrderId, targetVendorId.Value, ct);
            if (latestForward == null)
            {
                return new UpdateOrderVendorForwardStatusResultViewModel
                {
                    Success = false,
                    Message = "Vendor forward thread was not found."
                };
            }

            var normalizedStatus = NormalizeForwardStatus(model.Status);
            var now = DateTime.UtcNow;
            var isVendorActor = vendorUser != null;
            var actorName = currentUser?.FullName ?? vendorUser?.Name ?? "Agora Team";

            if (latestForward.IsLocked && normalizedStatus != ForwardStatusDelivered)
            {
                return new UpdateOrderVendorForwardStatusResultViewModel
                {
                    Success = false,
                    Message = "This vendor order is locked after the delivered workflow started.",
                    IsLocked = true
                };
            }

            if (normalizedStatus == ForwardStatusDelivered)
            {
                return await _vendorDeliveredService.MarkVendorDeliveredAsync(
                    model.OrderId,
                    targetVendorId.Value,
                    resolvedUserId,
                    actorName,
                    isVendorActor,
                    ct);
            }

            latestForward.FulfillmentStatus = normalizedStatus;
            latestForward.StatusUpdatedAt = now;
            latestForward.StatusUpdatedByUserId = resolvedUserId;
            latestForward.StatusUpdatedByName = actorName;
            latestForward.UpdatedAt = now;
            latestForward.UpdatedBy = actorName;

            ApplyViewStateAfterActorActivity(latestForward, isVendorActor, now);

            _unitOfWork.OrderVendorForwards.Update(latestForward);
            var updated = await _unitOfWork.SaveChangesAsync(ct) > 0;

            return new UpdateOrderVendorForwardStatusResultViewModel
            {
                Success = updated,
                Message = updated
                    ? "Vendor forward status updated successfully."
                    : "Failed to update vendor forward status.",
                IsLocked = latestForward.IsLocked,
                RequiresFinalization = false
            };
        }

        public async Task<OrderVendorNotificationListViewModel> GetForwardNotifications(string? userId, bool markAsRead, CancellationToken ct)
        {
            var viewer = await ResolveViewerContextAsync(userId, ct);
            var isVendorViewer = viewer.ViewerRole == ViewerRoleVendor;

            var notifications = new OrderVendorNotificationListViewModel
            {
                ViewerRole = viewer.ViewerRole
            };

            IQueryable<OrderVendorForward> forwardQuery = _unitOfWork.OrderVendorForwards.Query()
                .Where(forward => forward.IsDelete == 0 && forward.IsSuccess)
                .Include(forward => forward.Order)
                .Include(forward => forward.Vendor);

            if (viewer.Vendor != null)
                forwardQuery = forwardQuery.Where(forward => forward.VendorId == viewer.Vendor.Id);

            var forwards = await forwardQuery
                .OrderByDescending(forward => forward.CreatedAt)
                .ToListAsync(ct);

            if (!forwards.Any())
                return notifications;

            var latestForwards = forwards
                .GroupBy(forward => new { forward.OrderId, forward.VendorId })
                .Select(group => group.OrderByDescending(item => item.CreatedAt).First())
                .ToList();

            var vendorIds = latestForwards.Select(item => item.VendorId).Distinct().ToList();
            var orderIds = latestForwards.Select(item => item.OrderId).Distinct().ToList();

            var comments = await _unitOfWork.OrderVendorComments.Query()
                .Where(comment =>
                    comment.IsDelete == 0 &&
                    orderIds.Contains(comment.OrderId) &&
                    vendorIds.Contains(comment.VendorId))
                .OrderByDescending(comment => comment.CreatedAt)
                .ToListAsync(ct);

            if (markAsRead)
                await MarkThreadItemsAsReadAsync(latestForwards, comments, isVendorViewer, ct);

            var items = new List<OrderVendorNotificationViewModel>();

            if (isVendorViewer)
            {
                items.AddRange(latestForwards.Select(BuildVendorForwardNotification));
            }

            items.AddRange(
                comments
                    .Where(comment => string.Equals(comment.SenderRole, isVendorViewer ? SenderRoleAdmin : SenderRoleVendor, StringComparison.OrdinalIgnoreCase))
                    .Select(comment => BuildCommentNotification(comment, latestForwards, isVendorViewer)));

            items.AddRange(
                latestForwards
                    .Where(forward => forward.StatusUpdatedAt.HasValue)
                    .Select(forward => BuildStatusNotification(forward, isVendorViewer))
                    .Where(item => item != null)!
                    .Cast<OrderVendorNotificationViewModel>());

            notifications.Items = items
                .OrderByDescending(item => item.CreatedAt)
                .Take(25)
                .ToList();

            notifications.UnreadCount = notifications.Items.Count(item => item.IsUnread);

            return notifications;
        }

        private async Task<List<OrderViewModel>> LoadOrdersAsync(IQueryable<Order> query, long? vendorFilterId, CancellationToken ct)
        {
            var orders = await query
                .Include(order => order.OrderItems)
                .ToListAsync(ct);

            var mappedOrders = new List<OrderViewModel>();
            foreach (var order in orders)
            {
                var mapped = await BuildOrderViewModelAsync(order, vendorFilterId, ct);
                if (mapped != null)
                    mappedOrders.Add(mapped);
            }

            return mappedOrders;
        }

        private async Task<OrderViewModel?> BuildOrderViewModelAsync(Order order, long? vendorFilterId, CancellationToken ct)
        {
            var vendorByProductId = await GetVendorLookupByProductIdAsync(
                order.OrderItems.Select(item => item.ProductId),
                ct);
            var customer = await ResolveOrderCustomerAsync(order, ct);

            var items = order.OrderItems
                .Select(item =>
                {
                    vendorByProductId.TryGetValue(item.ProductId, out var vendorInfo);

                    return new OrderItemViewModel
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Name = item.Name,
                        ImageUrl = item.ImageUrl,
                        VendorId = vendorInfo?.VendorId,
                        VendorName = vendorInfo?.VendorName,
                        VendorEmail = vendorInfo?.VendorEmail,
                        VendorCompanyName = vendorInfo?.VendorCompanyName,
                    };
                })
                .ToList();

            var viewModel = new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : $"{order.FirstName} {order.LastName}".Trim(),
                CustomerEmail = customer?.Email,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Address = order.Address,
                Phone = order.Phone,
                City = order.City,
                ZipCode = order.ZipCode,
                Country = order.Country,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Tax = order.Tax,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CustomerQuery = order.CustomerQuery,
                OrderDate = order.OrderDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = items,
                VendorProgress = await BuildVendorProgressAsync(order.Id, vendorFilterId, ct)
            };

            return vendorFilterId.HasValue
                ? FilterOrderForVendor(viewModel, vendorFilterId.Value)
                : viewModel;
        }

        private async Task<List<OrderVendorProgressViewModel>> BuildVendorProgressAsync(long orderId, long? vendorFilterId, CancellationToken ct)
        {
            var forwards = await LoadSuccessfulForwardsAsync(orderId, vendorFilterId, ct);
            if (!forwards.Any())
                return new List<OrderVendorProgressViewModel>();

            var latestForwards = forwards
                .GroupBy(forward => forward.VendorId)
                .Select(group => group.OrderByDescending(item => item.CreatedAt).First())
                .ToList();

            var vendorIds = latestForwards.Select(item => item.VendorId).Distinct().ToList();
            var comments = await LoadCommentsAsync(orderId, vendorIds, ct);
            var deliveredLookup = await _unitOfWork.VendorDelivereds.Query()
                .Where(item => item.OrderId == orderId && item.IsDelete == 0 && vendorIds.Contains(item.VendorId))
                .Select(item => new
                {
                    item.Id,
                    item.VendorId,
                    item.IsFinalized,
                    item.ShipmentStatus
                })
                .ToDictionaryAsync(item => item.VendorId, item => item, ct);
            var isVendorViewer = vendorFilterId.HasValue;

            return latestForwards
                .Select(forward =>
                {
                    var vendorComments = comments.Where(comment => comment.VendorId == forward.VendorId).ToList();
                    deliveredLookup.TryGetValue(forward.VendorId, out var delivered);

                    return new OrderVendorProgressViewModel
                    {
                        ForwardId = forward.Id,
                        VendorId = forward.VendorId,
                        VendorName = forward.Vendor?.Name ?? "Vendor",
                        VendorEmail = forward.VendorEmail,
                        VendorCompanyName = forward.Vendor?.CompanyName,
                        FulfillmentStatus = NormalizeForwardStatus(forward.FulfillmentStatus),
                        IsLocked = forward.IsLocked,
                        VendorDeliveredId = delivered?.Id,
                        VendorDeliveredFinalized = delivered?.IsFinalized ?? false,
                        VendorDeliveredShipmentStatus = delivered?.ShipmentStatus,
                        ForwardedAt = forward.CreatedAt,
                        ForwardedByName = forward.ForwardedByName,
                        StatusUpdatedAt = forward.StatusUpdatedAt,
                        StatusUpdatedByName = forward.StatusUpdatedByName,
                        TotalComments = vendorComments.Count,
                        UnreadComments = CountUnreadComments(vendorComments, isVendorViewer),
                        HasUnreadForward = HasUnreadForward(forward, isVendorViewer),
                        HasUnreadStatusUpdate = HasUnreadStatusUpdate(forward, isVendorViewer),
                    };
                })
                .OrderByDescending(item => item.StatusUpdatedAt ?? item.ForwardedAt)
                .ToList();
        }

        private async Task<List<OrderVendorForward>> LoadSuccessfulForwardsAsync(long orderId, long? vendorId, CancellationToken ct)
        {
            var query = _unitOfWork.OrderVendorForwards.Query()
                .Where(forward => forward.OrderId == orderId && forward.IsDelete == 0 && forward.IsSuccess)
                .Include(forward => forward.Vendor)
                .AsQueryable();

            if (vendorId.HasValue)
                query = query.Where(forward => forward.VendorId == vendorId.Value);

            return await query
                .OrderByDescending(forward => forward.CreatedAt)
                .ToListAsync(ct);
        }

        private async Task<List<OrderVendorComment>> LoadCommentsAsync(long orderId, List<long> vendorIds, CancellationToken ct)
        {
            if (!vendorIds.Any())
                return new List<OrderVendorComment>();

            return await _unitOfWork.OrderVendorComments.Query()
                .Where(comment =>
                    comment.OrderId == orderId &&
                    comment.IsDelete == 0 &&
                    vendorIds.Contains(comment.VendorId))
                .OrderBy(comment => comment.CreatedAt)
                .ToListAsync(ct);
        }

        private static OrderVendorCommentThreadViewModel BuildThreadViewModel(OrderVendorForward latestForward, List<OrderVendorComment> comments, bool isVendorViewer)
        {
            var threadComments = comments.Where(comment => comment.VendorId == latestForward.VendorId).ToList();

            return new OrderVendorCommentThreadViewModel
            {
                ForwardId = latestForward.Id,
                VendorId = latestForward.VendorId,
                VendorName = latestForward.Vendor?.Name ?? "Vendor",
                VendorEmail = latestForward.VendorEmail,
                VendorCompanyName = latestForward.Vendor?.CompanyName,
                FulfillmentStatus = NormalizeForwardStatus(latestForward.FulfillmentStatus),
                ForwardedAt = latestForward.CreatedAt,
                ForwardedByName = latestForward.ForwardedByName,
                StatusUpdatedAt = latestForward.StatusUpdatedAt,
                StatusUpdatedByName = latestForward.StatusUpdatedByName,
                LastCommentAt = threadComments.LastOrDefault()?.CreatedAt ?? latestForward.StatusUpdatedAt ?? latestForward.CreatedAt,
                TotalComments = threadComments.Count,
                UnreadComments = CountUnreadComments(threadComments, isVendorViewer),
                HasUnreadForward = HasUnreadForward(latestForward, isVendorViewer),
                HasUnreadStatusUpdate = HasUnreadStatusUpdate(latestForward, isVendorViewer),
                CanComment = true,
                Comments = threadComments.Select(comment => new OrderVendorCommentViewModel
                {
                    Id = comment.Id,
                    OrderId = comment.OrderId,
                    VendorId = comment.VendorId,
                    SenderUserId = comment.SenderUserId,
                    SenderName = comment.SenderName,
                    SenderRole = comment.SenderRole,
                    Message = comment.Message,
                    CreatedAt = comment.CreatedAt,
                    IsRead = isVendorViewer
                        ? comment.VendorReadAt.HasValue
                        : comment.AdminReadAt.HasValue,
                }).ToList()
            };
        }

        private async Task MarkThreadItemsAsReadAsync(List<OrderVendorForward> forwards, List<OrderVendorComment> comments, bool isVendorViewer, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var hasChanges = false;

            foreach (var forward in forwards)
            {
                if (isVendorViewer)
                {
                    if (HasUnreadForward(forward, true) || HasUnreadStatusUpdate(forward, true))
                    {
                        forward.VendorLastViewedAt = now;
                        forward.UpdatedAt = now;
                        hasChanges = true;
                        _unitOfWork.OrderVendorForwards.Update(forward);
                    }
                }
                else if (HasUnreadStatusUpdate(forward, false))
                {
                    forward.AdminLastViewedAt = now;
                    forward.UpdatedAt = now;
                    hasChanges = true;
                    _unitOfWork.OrderVendorForwards.Update(forward);
                }
            }

            foreach (var comment in comments)
            {
                if (isVendorViewer)
                {
                    if (string.Equals(comment.SenderRole, SenderRoleAdmin, StringComparison.OrdinalIgnoreCase) && !comment.VendorReadAt.HasValue)
                    {
                        comment.VendorReadAt = now;
                        hasChanges = true;
                        _unitOfWork.OrderVendorComments.Update(comment);
                    }
                }
                else if (string.Equals(comment.SenderRole, SenderRoleVendor, StringComparison.OrdinalIgnoreCase) && !comment.AdminReadAt.HasValue)
                {
                    comment.AdminReadAt = now;
                    hasChanges = true;
                    _unitOfWork.OrderVendorComments.Update(comment);
                }
            }

            if (hasChanges)
                await _unitOfWork.SaveChangesAsync(ct);
        }

        private async Task<(string ViewerRole, Vendor? Vendor)> ResolveViewerContextAsync(string? userId, CancellationToken ct)
        {
            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? userId;
            var vendor = await ResolveVendorUserAsync(resolvedUserId, ct);

            return vendor == null
                ? (ViewerRoleAdmin, null)
                : (ViewerRoleVendor, vendor);
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

        private async Task<OrderVendorForward?> GetLatestForwardAsync(long orderId, long vendorId, CancellationToken ct)
        {
            return await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward =>
                    forward.OrderId == orderId &&
                    forward.VendorId == vendorId &&
                    forward.IsDelete == 0 &&
                    forward.IsSuccess)
                .OrderByDescending(forward => forward.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        private static long? ResolveTargetVendorId(Vendor? vendorUser, long? requestedVendorId)
        {
            if (vendorUser != null)
            {
                if (requestedVendorId.HasValue && requestedVendorId.Value != vendorUser.Id)
                    return null;

                return vendorUser.Id;
            }

            return requestedVendorId;
        }

        private static int CountUnreadComments(List<OrderVendorComment> comments, bool isVendorViewer)
        {
            return isVendorViewer
                ? comments.Count(comment =>
                    string.Equals(comment.SenderRole, SenderRoleAdmin, StringComparison.OrdinalIgnoreCase) &&
                    !comment.VendorReadAt.HasValue)
                : comments.Count(comment =>
                    string.Equals(comment.SenderRole, SenderRoleVendor, StringComparison.OrdinalIgnoreCase) &&
                    !comment.AdminReadAt.HasValue);
        }

        private static bool HasUnreadForward(OrderVendorForward forward, bool isVendorViewer)
        {
            if (!isVendorViewer)
                return false;

            if (!forward.CreatedAt.HasValue)
                return false;

            return !forward.VendorLastViewedAt.HasValue || forward.CreatedAt.Value > forward.VendorLastViewedAt.Value;
        }

        private static bool HasUnreadStatusUpdate(OrderVendorForward forward, bool isVendorViewer)
        {
            if (!forward.StatusUpdatedAt.HasValue)
                return false;

            var lastViewedAt = isVendorViewer ? forward.VendorLastViewedAt : forward.AdminLastViewedAt;
            return !lastViewedAt.HasValue || forward.StatusUpdatedAt.Value > lastViewedAt.Value;
        }

        private static string NormalizeForwardStatus(string? status)
        {
            var normalized = status?.Trim();
            if (string.Equals(normalized, ForwardStatusLegacyDelivery, StringComparison.OrdinalIgnoreCase))
                return ForwardStatusDelivered;

            var match = AllowedForwardStatuses.FirstOrDefault(item =>
                string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase));

            return match ?? ForwardStatusPending;
        }

        private async Task<Dictionary<long, ProductVendorLookup>> GetVendorLookupByProductIdAsync(IEnumerable<long> productIds, CancellationToken ct)
        {
            var ids = productIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<long, ProductVendorLookup>();

            return await _unitOfWork.Products.Query()
                .Where(product => product.IsDelete == 0 && ids.Contains(product.Id))
                .Select(product => new
                {
                    product.Id,
                    product.VendorId,
                    VendorName = product.Vendor != null ? product.Vendor.Name : null,
                    VendorEmail = product.Vendor != null ? product.Vendor.Email : null,
                    VendorCompanyName = product.Vendor != null ? product.Vendor.CompanyName : null,
                })
                .ToDictionaryAsync(
                    product => product.Id,
                    product => new ProductVendorLookup
                    {
                        VendorId = product.VendorId,
                        VendorName = product.VendorName,
                        VendorEmail = product.VendorEmail,
                        VendorCompanyName = product.VendorCompanyName,
                    },
                    ct);
        }

        private OrderViewModel? FilterOrderForVendor(OrderViewModel order, long vendorId)
        {
            var vendorItems = order.Items
                .Where(item => item.VendorId == vendorId)
                .ToList();

            if (vendorItems.Count == 0)
                return null;

            var vendorSubTotal = vendorItems.Sum(item => item.UnitPrice * item.Quantity);

            return new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Address = order.Address,
                Phone = order.Phone,
                City = order.City,
                ZipCode = order.ZipCode,
                Country = order.Country,
                SubTotal = vendorSubTotal,
                ShippingFee = 0,
                Tax = 0,
                TotalAmount = vendorSubTotal,
                Status = order.Status,
                CustomerQuery = order.CustomerQuery,
                OrderDate = order.OrderDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = vendorItems,
                VendorProgress = order.VendorProgress
                    .Where(item => item.VendorId == vendorId)
                    .ToList()
            };
        }

        private async Task<Vendor?> ResolveVendorUserAsync(string? userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _unitOfWork.Vendors.Query()
                .Where(vendor => vendor.IsDelete == 0 && vendor.IsActive && vendor.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<Customer?> ResolveCustomerUserAsync(string? userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _unitOfWork.Customers.Query()
                .Where(customer => customer.IsDelete == 0 && customer.IsActive && customer.UserId == userId)
                .FirstOrDefaultAsync(ct);
        }

        private async Task<Customer?> ResolveOrderCustomerAsync(Order order, CancellationToken ct)
        {
            if (order.Customer != null)
                return order.Customer;

            if (!order.CustomerId.HasValue)
                return null;

            return await _unitOfWork.Customers.Query()
                .Where(customer => customer.Id == order.CustomerId.Value && customer.IsDelete == 0)
                .FirstOrDefaultAsync(ct);
        }

        private async Task AutoForwardOrderToVendorsAsync(Order order, CancellationToken ct)
        {
            var fullOrder = await BuildOrderViewModelAsync(order, null, ct);
            if (fullOrder == null)
                return;

            var vendorIds = fullOrder.Items
                .Where(item => item.VendorId.HasValue)
                .Select(item => item.VendorId!.Value)
                .Distinct()
                .ToList();

            if (vendorIds.Count == 0)
                return;

            var vendors = await _unitOfWork.Vendors.Query()
                .Where(vendor => vendor.IsDelete == 0 && vendor.IsActive && vendorIds.Contains(vendor.Id))
                .ToListAsync(ct);

            if (vendors.Count == 0)
                return;

            var successfulVendorIds = await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward =>
                    forward.OrderId == order.Id &&
                    forward.IsDelete == 0 &&
                    forward.IsSuccess &&
                    vendorIds.Contains(forward.VendorId))
                .Select(forward => forward.VendorId)
                .Distinct()
                .ToListAsync(ct);

            var currentUser = await _workContext.CurrentUserAsync();

            foreach (var vendor in vendors.Where(item => !successfulVendorIds.Contains(item.Id)))
            {
                var vendorOrder = FilterOrderForVendor(fullOrder, vendor.Id);
                if (vendorOrder == null)
                    continue;

                var message = BuildVendorForwardMessage(vendor, vendorOrder);

                await SendOrderToVendorAsync(
                    vendorOrder,
                    vendor,
                    message,
                    currentUser?.Id,
                    currentUser?.FullName,
                    ct);
            }
        }

        private async Task<bool> SendOrderToVendorAsync(
            OrderViewModel order,
            Vendor vendor,
            string message,
            string? forwardedByUserId,
            string? forwardedByName,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var actorName = string.IsNullOrWhiteSpace(forwardedByName) ? "System" : forwardedByName;
            var actorId = string.IsNullOrWhiteSpace(forwardedByUserId) ? null : forwardedByUserId;

            var forwardLog = new OrderVendorForward
            {
                OrderId = order.Id,
                VendorId = vendor.Id,
                OrderNumber = order.OrderNumber ?? string.Empty,
                VendorEmail = vendor.Email,
                ForwardedByUserId = actorId,
                ForwardedByName = actorName,
                FulfillmentStatus = ForwardStatusPending,
                AdminLastViewedAt = now,
                VendorLastViewedAt = null,
                IsDelete = 0,
                CreatedAt = now,
                CreatedBy = actorName,
            };

            bool success;
            try
            {
                byte[] pdfBytes = OrderPdfGenerator.GenerateOrderRequestPdf(order);
                string attachmentName = $"Order_Request_{order.OrderNumber ?? order.Id.ToString()}_{vendor.Id}.pdf";
                string subject = $"Order Fulfillment Request - #{order.OrderNumber ?? order.Id.ToString()}";

                success = await _emailService.SendEmailAsync(
                    vendor.Email,
                    subject,
                    message,
                    pdfBytes,
                    attachmentName,
                    "Fulfillment Required",
                    "Stock Fulfillment Request");
                forwardLog.IsSuccess = success;

                if (!success)
                    forwardLog.ErrorMessage = "Email service returned false.";
            }
            catch (Exception ex)
            {
                success = false;
                forwardLog.IsSuccess = false;
                forwardLog.ErrorMessage = ex.Message;
            }

            await _unitOfWork.OrderVendorForwards.AddAsync(forwardLog, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            Console.WriteLine(success
                ? $"[EMAIL SUCCESS] Order #{order.Id} forwarded to {vendor.Email}."
                : $"[EMAIL ERROR] Failed to send order #{order.Id} to {vendor.Email}");

            return success;
        }

        private static OrderVendorNotificationViewModel BuildVendorForwardNotification(OrderVendorForward forward)
        {
            return new OrderVendorNotificationViewModel
            {
                Id = $"forward-{forward.Id}",
                Type = "forward",
                OrderId = forward.OrderId,
                OrderNumber = forward.Order?.OrderNumber ?? forward.OrderNumber,
                VendorId = forward.VendorId,
                VendorName = forward.Vendor?.Name ?? "Vendor",
                ViewerRole = ViewerRoleVendor,
                Title = "New forwarded order",
                Message = $"Order #{forward.Order?.OrderNumber ?? forward.OrderNumber} was forwarded to your vendor queue.",
                Status = NormalizeForwardStatus(forward.FulfillmentStatus),
                IsUnread = HasUnreadForward(forward, true),
                CreatedAt = forward.CreatedAt,
            };
        }

        private static OrderVendorNotificationViewModel BuildCommentNotification(OrderVendorComment comment, List<OrderVendorForward> latestForwards, bool isVendorViewer)
        {
            var forward = latestForwards.FirstOrDefault(item => item.OrderId == comment.OrderId && item.VendorId == comment.VendorId);
            var message = comment.Message.Length > 120
                ? $"{comment.Message[..117]}..."
                : comment.Message;

            return new OrderVendorNotificationViewModel
            {
                Id = $"comment-{comment.Id}",
                Type = "comment",
                OrderId = comment.OrderId,
                OrderNumber = forward?.Order?.OrderNumber ?? forward?.OrderNumber,
                VendorId = comment.VendorId,
                VendorName = forward?.Vendor?.Name ?? "Vendor",
                ViewerRole = isVendorViewer ? ViewerRoleVendor : ViewerRoleAdmin,
                Title = isVendorViewer ? "New admin comment" : "New vendor comment",
                Message = message,
                Status = forward != null ? NormalizeForwardStatus(forward.FulfillmentStatus) : null,
                IsUnread = isVendorViewer ? !comment.VendorReadAt.HasValue : !comment.AdminReadAt.HasValue,
                CreatedAt = comment.CreatedAt,
            };
        }

        private static OrderVendorNotificationViewModel? BuildStatusNotification(OrderVendorForward forward, bool isVendorViewer)
        {
            if (!forward.StatusUpdatedAt.HasValue)
                return null;

            var isUnread = HasUnreadStatusUpdate(forward, isVendorViewer);
            var message = isVendorViewer
                ? $"Admin updated order #{forward.Order?.OrderNumber ?? forward.OrderNumber} to {NormalizeForwardStatus(forward.FulfillmentStatus)}."
                : $"{forward.Vendor?.Name ?? "Vendor"} updated order #{forward.Order?.OrderNumber ?? forward.OrderNumber} to {NormalizeForwardStatus(forward.FulfillmentStatus)}.";

            return new OrderVendorNotificationViewModel
            {
                Id = $"status-{forward.Id}-{forward.StatusUpdatedAt.Value.Ticks}",
                Type = "status",
                OrderId = forward.OrderId,
                OrderNumber = forward.Order?.OrderNumber ?? forward.OrderNumber,
                VendorId = forward.VendorId,
                VendorName = forward.Vendor?.Name ?? "Vendor",
                ViewerRole = isVendorViewer ? ViewerRoleVendor : ViewerRoleAdmin,
                Title = "Vendor progress updated",
                Message = message,
                Status = NormalizeForwardStatus(forward.FulfillmentStatus),
                IsUnread = isUnread,
                CreatedAt = forward.StatusUpdatedAt,
            };
        }

        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD-{timestamp}-{random}";
        }

        private static string BuildVendorForwardMessage(Vendor vendor, OrderViewModel order)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Dear {vendor.Name} ({vendor.CompanyName}),");
            builder.AppendLine();
            builder.AppendLine("Please fulfill the following order items assigned to your vendor account.");
            builder.AppendLine();
            builder.AppendLine($"Order Reference: #{order.OrderNumber ?? order.Id.ToString()}");
            builder.AppendLine("Items:");

            foreach (var item in order.Items)
                builder.AppendLine($"- {item.Name} (Qty: {item.Quantity})");

            builder.AppendLine();
            builder.AppendLine("Customer Query / Instructions:");
            builder.AppendLine(string.IsNullOrWhiteSpace(order.CustomerQuery)
                ? "None"
                : ConvertHtmlToPlainText(order.CustomerQuery));
            builder.AppendLine();
            builder.AppendLine("Please confirm and continue with the normal forwarding comment process if anything needs clarification.");
            builder.AppendLine();
            builder.AppendLine("Regards,");
            builder.AppendLine("Agora Food");

            return builder.ToString().Trim();
        }

        private static OrderViewModel BuildCustomerOrderEmailSnapshot(Order order, string? customerEmail)
        {
            return new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.FirstName} {order.LastName}".Trim(),
                CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim(),
                FirstName = order.FirstName,
                LastName = order.LastName,
                Address = order.Address,
                Phone = order.Phone,
                City = order.City,
                ZipCode = order.ZipCode,
                Country = order.Country,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Tax = order.Tax,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CustomerQuery = order.CustomerQuery,
                OrderDate = order.OrderDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.OrderItems.Select(item => new OrderItemViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Name = item.Name,
                    ImageUrl = item.ImageUrl
                }).ToList()
            };
        }

        private async Task SendCustomerOrderConfirmationInBackgroundAsync(OrderViewModel order)
        {
            try
            {
                var customerEmail = ResolveCustomerNotificationEmail(order, null);
                if (string.IsNullOrWhiteSpace(customerEmail))
                    return;

                var pdfBytes = OrderPdfGenerator.GenerateCustomerInvoicePdf(order);
                var orderReference = order.OrderNumber ?? order.Id.ToString();
                var subject = $"Agora Food order confirmation - #{orderReference}";
                var attachmentName = $"AgoraFood_Invoice_{orderReference}.pdf";
                var customerName = ResolveCustomerDisplayName(order);
                var message = BuildCustomerOrderConfirmationMessage(customerName, order);

                var emailSent = await _emailService.SendEmailAsync(
                    customerEmail,
                    subject,
                    message,
                    pdfBytes,
                    attachmentName,
                    "Order Confirmed",
                    "Your Order Invoice");

                Console.WriteLine(emailSent
                    ? $"[EMAIL SUCCESS] Order invoice for #{orderReference} sent to {customerEmail}."
                    : $"[EMAIL ERROR] Failed to send customer invoice for order #{orderReference} to {customerEmail}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send customer invoice in background for order #{order.Id}: {ex.Message}");
            }
        }

        private static string ResolveCustomerNotificationEmail(OrderViewModel order, string? fallbackEmail)
        {
            if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
                return order.CustomerEmail.Trim();

            return string.IsNullOrWhiteSpace(fallbackEmail)
                ? string.Empty
                : fallbackEmail.Trim();
        }

        private static string ResolveCustomerDisplayName(OrderViewModel order)
        {
            if (!string.IsNullOrWhiteSpace(order.CustomerName))
                return order.CustomerName.Trim();

            var fullName = $"{order.FirstName} {order.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? "Customer" : fullName;
        }

        private static string BuildCustomerOrderConfirmationMessage(string customerName, OrderViewModel order)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Dear {customerName},");
            builder.AppendLine();
            builder.AppendLine("Thank you for placing your order with Agora Food.");
            builder.AppendLine("Your order has been received successfully, and your invoice is attached to this email.");
            builder.AppendLine();
            builder.AppendLine($"Order Reference: #{order.OrderNumber ?? order.Id.ToString()}");
            builder.AppendLine($"Order Date: {order.OrderDate:dd MMM yyyy HH:mm} UTC");
            builder.AppendLine($"Status: {order.Status}");
            builder.AppendLine();
            builder.AppendLine("Ordered Items:");

            foreach (var item in order.Items)
                builder.AppendLine($"- {item.Name} (Qty: {item.Quantity}) x ${item.UnitPrice:0.00} = ${(item.UnitPrice * item.Quantity):0.00}");

            builder.AppendLine();
            builder.AppendLine($"Sub Total: ${order.SubTotal:0.00}");
            builder.AppendLine($"Shipping Fee: ${order.ShippingFee:0.00}");
            builder.AppendLine($"Tax: ${order.Tax:0.00}");
            builder.AppendLine($"Grand Total: ${order.TotalAmount:0.00}");
            builder.AppendLine();
            builder.AppendLine("Shipping Address:");
            builder.AppendLine($"{order.FirstName} {order.LastName}".Trim());
            builder.AppendLine(order.Address);
            builder.AppendLine($"{order.City}, {order.ZipCode}");
            builder.AppendLine(order.Country);
            builder.AppendLine($"Phone: {order.Phone}");

            if (!string.IsNullOrWhiteSpace(order.CustomerQuery))
            {
                builder.AppendLine();
                builder.AppendLine("Customer Notes:");
                builder.AppendLine(ConvertHtmlToPlainText(order.CustomerQuery));
            }

            builder.AppendLine();
            builder.AppendLine("We appreciate your business and will keep you updated if there is any change to your order.");
            builder.AppendLine();
            builder.AppendLine("Best regards,");
            builder.AppendLine("Agora Food");

            return builder.ToString().Trim();
        }

        private static string ConvertHtmlToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var withBreaks = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</p\\s*>", "\n", RegexOptions.IgnoreCase);
            withBreaks = Regex.Replace(withBreaks, "</li\\s*>", "\n", RegexOptions.IgnoreCase);

            var noTags = Regex.Replace(withBreaks, "<.*?>", string.Empty);
            return WebUtility.HtmlDecode(noTags).Trim();
        }

        private sealed class ProductVendorLookup
        {
            public long? VendorId { get; set; }
            public string? VendorName { get; set; }
            public string? VendorEmail { get; set; }
            public string? VendorCompanyName { get; set; }
        }
    }
}
