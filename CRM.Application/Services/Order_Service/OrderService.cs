using CRM.Application.Common.Pagination;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Email_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Order_Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IWorkContext _workContext;

        public OrderService(IUnitOfWork unitOfWork, IEmailService emailService, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _workContext = workContext;
        }

        public async Task<int> CreateOrder(OrderViewModel model, CancellationToken ct)
        {
            try
            {
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
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
            var orders = _unitOfWork.Orders.Query()
                .Where(o => o.IsDelete == 0)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Address = o.Address,
                    Phone = o.Phone,
                    City = o.City,
                    ZipCode = o.ZipCode,
                    Country = o.Country,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    Tax = o.Tax,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CustomerQuery = o.CustomerQuery,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Name = oi.Name,
                        ImageUrl = oi.ImageUrl
                    }).ToList()
                });

            return new OrderViewModel { OrderList = orders };
        }

        public async Task<OrderViewModel> GetOrderById(long id, CancellationToken ct)
        {
            var order = await _unitOfWork.Orders.Query()
                .Where(o => o.Id == id && o.IsDelete == 0)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(ct);

            if (order == null) return null;

            return new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
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
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Name = oi.Name,
                    ImageUrl = oi.ImageUrl
                }).ToList()
            };
        }

        public async Task<int> UpdateOrderStatus(UpdateOrderStatusViewModel model, CancellationToken ct)
        {
            try
            {
                var order = await _unitOfWork.Orders.Query()
                    .FirstOrDefaultAsync(o => o.Id == model.Id && o.IsDelete == 0, ct);

                if (order == null) return 1;

                order.Status = model.Status.ToLower();
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.SaveChangesAsync(ct);

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
                    .FirstOrDefaultAsync(o => o.Id == model.Id && o.IsDelete == 0, ct);

                if (order == null) return false;

                order.CustomerQuery = string.IsNullOrWhiteSpace(model.CustomerQuery)
                    ? null
                    : model.CustomerQuery.Trim();
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.SaveChangesAsync(ct);

                return result > 0;
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
                    .FirstOrDefaultAsync(o => o.Id == id && o.IsDelete == 0, ct);

                if (order == null) return false;

                order.IsDelete = 1;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.SaveChangesAsync(ct);

                return result > 0;
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
                .Where(o => o.IsDelete == 0)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(searchTerm) ||
                    o.FirstName.ToLower().Contains(searchTerm) ||
                    o.LastName.ToLower().Contains(searchTerm) ||
                    o.Phone.Contains(searchTerm) ||
                    o.Status.ToLower().Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync(ct);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Address = o.Address,
                    Phone = o.Phone,
                    City = o.City,
                    ZipCode = o.ZipCode,
                    Country = o.Country,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    Tax = o.Tax,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CustomerQuery = o.CustomerQuery,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Name = oi.Name,
                        ImageUrl = oi.ImageUrl
                    }).ToList()
                })
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            return new PaginatedResult<OrderViewModel>
            {
                Items = orders,
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
            return await _unitOfWork.Orders.Query()
                .Where(o => o.Phone == phone && o.IsDelete == 0)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Address = o.Address,
                    Phone = o.Phone,
                    City = o.City,
                    ZipCode = o.ZipCode,
                    Country = o.Country,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    Tax = o.Tax,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CustomerQuery = o.CustomerQuery,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Name = oi.Name,
                        ImageUrl = oi.ImageUrl
                    }).ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<List<OrderViewModel>> GetOrdersByStatus(string status, CancellationToken ct)
        {
            return await _unitOfWork.Orders.Query()
                .Where(o => o.Status.ToLower() == status.ToLower() && o.IsDelete == 0)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Address = o.Address,
                    Phone = o.Phone,
                    City = o.City,
                    ZipCode = o.ZipCode,
                    Country = o.Country,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    Tax = o.Tax,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CustomerQuery = o.CustomerQuery,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Name = oi.Name,
                        ImageUrl = oi.ImageUrl
                    }).ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<List<OrderViewModel>> GetMyOrders(string userId, CancellationToken ct)
        {
            var vendor = await _unitOfWork.Vendors.Query()
                .Where(v => v.IsDelete == 0 && v.IsActive && v.UserId == userId)
                .FirstOrDefaultAsync(ct);

            if (vendor == null)
                return new List<OrderViewModel>();

            var forwardedOrderIds = await _unitOfWork.OrderVendorForwards.Query()
                .Where(f => f.IsDelete == 0 && f.VendorId == vendor.Id && f.IsSuccess)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.OrderId)
                .Distinct()
                .ToListAsync(ct);

            if (forwardedOrderIds.Count == 0)
                return new List<OrderViewModel>();

            return await _unitOfWork.Orders.Query()
                .Where(o => o.IsDelete == 0 && forwardedOrderIds.Contains(o.Id))
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Address = o.Address,
                    Phone = o.Phone,
                    City = o.City,
                    ZipCode = o.ZipCode,
                    Country = o.Country,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    Tax = o.Tax,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CustomerQuery = o.CustomerQuery,
                    OrderDate = o.OrderDate,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Name = oi.Name,
                        ImageUrl = oi.ImageUrl
                    }).ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<bool> ForwardToVendor(ForwardOrderViewModel model, CancellationToken ct)
        {
            try
            {
                var order = await GetOrderById(model.OrderId, ct);
                if (order == null) return false;

                if (!long.TryParse(model.VendorId, out var vendorId))
                    return false;

                var vendor = await _unitOfWork.Vendors.Query()
                    .Where(v => v.Id == vendorId && v.IsDelete == 0 && v.IsActive)
                    .FirstOrDefaultAsync(ct);

                if (vendor == null) return false;

                var currentUser = await _workContext.CurrentUserAsync();
                var forwardedByUserId = currentUser?.Id ?? model.UserId;
                var vendorEmail = vendor.Email;

                var forwardLog = new OrderVendorForward
                {
                    OrderId = model.OrderId,
                    VendorId = vendorId,
                    OrderNumber = order.OrderNumber ?? string.Empty,
                    VendorEmail = vendorEmail,
                    ForwardedByUserId = forwardedByUserId,
                    ForwardedByName = currentUser?.FullName,
                    IsDelete = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser?.FullName ?? forwardedByUserId,
                };

                bool success;
                try
                {
                    byte[] pdfBytes = OrderPdfGenerator.GenerateOrderRequestPdf(order);
                    string attachmentName = $"Order_Request_{order.OrderNumber ?? order.Id.ToString()}.pdf";
                    string subject = $"Order Fulfillment Request - #{order.OrderNumber ?? order.Id.ToString()}";

                    success = await _emailService.SendEmailAsync(vendorEmail, subject, model.Message, pdfBytes, attachmentName);
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
                    ? $"[EMAIL SUCCESS] Order #{model.OrderId} forwarded to {vendorEmail}."
                    : $"[EMAIL ERROR] Failed to send order #{model.OrderId} to {vendorEmail}");

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ForwardToVendor service: {ex.Message}");
                return false;
            }
        }

        public async Task<OrderVendorCommentResponseViewModel> GetForwardComments(long orderId, string? userId, CancellationToken ct)
        {
            var response = new OrderVendorCommentResponseViewModel { OrderId = orderId };

            var order = await _unitOfWork.Orders.Query()
                .Where(orderItem => orderItem.Id == orderId && orderItem.IsDelete == 0)
                .FirstOrDefaultAsync(ct);

            if (order == null)
                return response;

            response.OrderNumber = order.OrderNumber;

            var currentUser = await _workContext.CurrentUserAsync();
            var resolvedUserId = currentUser?.Id ?? userId;

            var vendorViewer = string.IsNullOrWhiteSpace(resolvedUserId)
                ? null
                : await _unitOfWork.Vendors.Query()
                    .Where(vendor => vendor.IsDelete == 0 && vendor.IsActive && vendor.UserId == resolvedUserId)
                    .FirstOrDefaultAsync(ct);

            response.ViewerRole = vendorViewer == null ? "admin" : "vendor";

            var forwards = await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward => forward.OrderId == orderId && forward.IsDelete == 0 && forward.IsSuccess)
                .Include(forward => forward.Vendor)
                .OrderByDescending(forward => forward.CreatedAt)
                .ToListAsync(ct);

            if (vendorViewer != null)
                forwards = forwards.Where(forward => forward.VendorId == vendorViewer.Id).ToList();

            var vendorIds = forwards.Select(forward => forward.VendorId).Distinct().ToList();
            if (!vendorIds.Any())
                return response;

            var comments = await _unitOfWork.OrderVendorComments.Query()
                .Where(comment =>
                    comment.OrderId == orderId &&
                    comment.IsDelete == 0 &&
                    vendorIds.Contains(comment.VendorId))
                .OrderBy(comment => comment.CreatedAt)
                .ToListAsync(ct);

            response.Threads = forwards
                .GroupBy(forward => forward.VendorId)
                .Select(group =>
                {
                    var latestForward = group.OrderByDescending(item => item.CreatedAt).First();
                    var threadComments = comments.Where(comment => comment.VendorId == group.Key).ToList();

                    return new OrderVendorCommentThreadViewModel
                    {
                        VendorId = group.Key,
                        VendorName = latestForward.Vendor?.Name ?? "Vendor",
                        VendorEmail = latestForward.VendorEmail,
                        VendorCompanyName = latestForward.Vendor?.CompanyName,
                        ForwardedAt = latestForward.CreatedAt,
                        ForwardedByName = latestForward.ForwardedByName,
                        LastCommentAt = threadComments.LastOrDefault()?.CreatedAt ?? latestForward.CreatedAt,
                        TotalComments = threadComments.Count,
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
                            CreatedAt = comment.CreatedAt
                        }).ToList()
                    };
                })
                .OrderByDescending(thread => thread.LastCommentAt ?? thread.ForwardedAt)
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

            var vendorUser = string.IsNullOrWhiteSpace(resolvedUserId)
                ? null
                : await _unitOfWork.Vendors.Query()
                    .Where(vendor => vendor.IsDelete == 0 && vendor.IsActive && vendor.UserId == resolvedUserId)
                    .FirstOrDefaultAsync(ct);

            long targetVendorId;
            if (vendorUser != null)
            {
                targetVendorId = vendorUser.Id;

                if (model.VendorId.HasValue && model.VendorId.Value != vendorUser.Id)
                    return false;
            }
            else if (model.VendorId.HasValue)
            {
                targetVendorId = model.VendorId.Value;
            }
            else
            {
                return false;
            }

            var latestForward = await _unitOfWork.OrderVendorForwards.Query()
                .Where(forward =>
                    forward.OrderId == model.OrderId &&
                    forward.VendorId == targetVendorId &&
                    forward.IsDelete == 0 &&
                    forward.IsSuccess)
                .OrderByDescending(forward => forward.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (latestForward == null)
                return false;

            var comment = new OrderVendorComment
            {
                OrderId = model.OrderId,
                VendorId = targetVendorId,
                OrderVendorForwardId = latestForward.Id,
                SenderUserId = resolvedUserId,
                SenderName = currentUser?.FullName ?? vendorUser?.Name ?? "Agora Team",
                SenderRole = vendorUser == null ? "admin" : "vendor",
                Message = message,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUser?.FullName ?? resolvedUserId,
                IsDelete = 0
            };

            await _unitOfWork.OrderVendorComments.AddAsync(comment, ct);
            return await _unitOfWork.SaveChangesAsync(ct) > 0;
        }

        private string GenerateOrderNumber()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"ORD-{timestamp}-{random}";
        }
    }
}
