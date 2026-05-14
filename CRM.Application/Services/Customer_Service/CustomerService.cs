using CRM.Application.Common.Pagination;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Auth_Service;
using CRM.Application.Services.Order_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Customer_Service
{
    public class CustomerService : ICustomerService
    {
        private const string CustomerRoleName = "Customer";
        private const string LegacyOrderStatusProcessing = "processing";
        private const string OrderStatusAccepted = "accept order";

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;

        public CustomerService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IWorkContext workContext,
            IOrderService orderService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _workContext = workContext;
            _orderService = orderService;
        }

        public async Task<CustomerRegistrationResultVm> RegisterCheckoutAsync(CustomerCheckoutRegistrationVm model, CancellationToken cancellationToken)
        {
            NormalizeRegistrationModel(model);
            await EnsureCustomerEmailAvailableAsync(model.Email, null, null, cancellationToken);
            await EnsureCustomerPhoneAvailableAsync(model.Phone, null, cancellationToken);
            await EnsureRoleExistsAsync(CustomerRoleName);

            ApplicationUser? user = null;
            Customer? customer = null;

            try
            {
                var fullName = BuildFullName(model.FirstName, model.LastName);
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PhoneNumber = model.Phone,
                    FullName = fullName,
                    EntryBy = "Customer Checkout",
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                    throw new Exception(string.Join(" ", createResult.Errors.Select(item => item.Description)));

                var roleResult = await _userManager.AddToRoleAsync(user, CustomerRoleName);
                if (!roleResult.Succeeded)
                    throw new Exception(string.Join(" ", roleResult.Errors.Select(item => item.Description)));

                customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    ZipCode = model.ZipCode,
                    Country = model.Country,
                    UserId = user.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Customer Checkout",
                    IsDelete = 0
                };

                await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
                var roles = await _userManager.GetRolesAsync(user);

                return new CustomerRegistrationResultVm
                {
                    Message = "Customer account created successfully.",
                    Customer = MapProfile(customer),
                    Login = BuildLoginResponse(user, roles, accessToken, refreshToken.RefreshToken)
                };
            }
            catch
            {
                if (customer != null && customer.Id > 0)
                {
                    _unitOfWork.Customers.Remove(customer);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                if (user != null)
                    await _userManager.DeleteAsync(user);

                throw;
            }
        }

        public async Task<CustomerProfileVm> GetCurrentProfileAsync(CancellationToken cancellationToken)
        {
            var customer = await GetCurrentCustomerEntityAsync(cancellationToken);
            return MapProfile(customer);
        }

        public async Task<CustomerProfileVm> UpdateCurrentProfileAsync(UpdateCustomerProfileVm model, CancellationToken cancellationToken)
        {
            NormalizeProfileModel(model);

            var customer = await GetCurrentCustomerEntityAsync(cancellationToken);
            await EnsureCustomerPhoneAvailableAsync(model.Phone, customer.Id, cancellationToken);

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.ZipCode = model.ZipCode;
            customer.Country = model.Country;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = "Customer";

            var user = await _userManager.FindByIdAsync(customer.UserId)
                ?? throw new Exception("Customer user account not found.");

            user.FullName = BuildFullName(model.FirstName, model.LastName);
            user.PhoneNumber = model.Phone;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
                throw new Exception(string.Join(" ", updateUserResult.Errors.Select(item => item.Description)));

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapProfile(customer);
        }

        public async Task<CustomerProfileVm> UpdateByAdminAsync(UpdateCustomerAdminVm model, CancellationToken cancellationToken)
        {
            NormalizeAdminUpdateModel(model);
            var customer = await _unitOfWork.Customers.Query()
                .FirstOrDefaultAsync(item => item.Id == model.Id && item.IsDelete == 0, cancellationToken)
                ?? throw new Exception("Customer not found.");
            await EnsureCustomerEmailAvailableAsync(model.Email, customer.Id, customer.UserId, cancellationToken);
            await EnsureCustomerPhoneAvailableAsync(model.Phone, customer.Id, cancellationToken);
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.ZipCode = model.ZipCode;
            customer.Country = model.Country;
            customer.IsActive = model.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = "Admin";
            var user = await _userManager.FindByIdAsync(customer.UserId)
                ?? throw new Exception("Customer user account not found.");
            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedEmail = model.Email.ToUpperInvariant();
            user.NormalizedUserName = model.Email.ToUpperInvariant();
            user.FullName = BuildFullName(model.FirstName, model.LastName);
            user.PhoneNumber = model.Phone;
            user.LockoutEnabled = !model.IsActive;
            user.LockoutEnd = model.IsActive ? null : DateTimeOffset.MaxValue;
            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
                throw new Exception(string.Join(" ", updateUserResult.Errors.Select(item => item.Description)));
            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapProfile(customer);
        }
        public async Task<List<OrderViewModel>> GetMyOrdersAsync(CancellationToken cancellationToken)
        {
            var currentUser = await _workContext.CurrentUserAsync() ?? throw new Exception("Unauthorized request.");
            return await _orderService.GetOrdersByCustomerUserId(currentUser.Id, cancellationToken);
        }

        public async Task<CustomerFeedbackVm> CreateFeedbackAsync(CreateCustomerFeedbackVm model, CancellationToken cancellationToken)
        {
            NormalizeFeedbackModel(model);

            var customer = await GetCurrentCustomerEntityAsync(cancellationToken);
            Order? linkedOrder = null;

            if (model.OrderId.HasValue)
            {
                linkedOrder = await _unitOfWork.Orders.Query()
                    .FirstOrDefaultAsync(
                        item => item.Id == model.OrderId.Value && item.IsDelete == 0 && item.CustomerId == customer.Id,
                        cancellationToken)
                    ?? throw new Exception("The selected order was not found for this customer.");
            }

            var feedback = new CustomerFeedback
            {
                CustomerId = customer.Id,
                OrderId = model.OrderId,
                Rating = model.Rating,
                Subject = model.Subject,
                Message = model.Message,
                Status = "new",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = BuildFullName(customer.FirstName, customer.LastName),
                IsDelete = 0
            };

            await _unitOfWork.CustomerFeedbacks.AddAsync(feedback, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapFeedback(feedback, linkedOrder);
        }

        public async Task<List<CustomerFeedbackVm>> GetMyFeedbacksAsync(CancellationToken cancellationToken)
        {
            var customer = await GetCurrentCustomerEntityAsync(cancellationToken);

            var feedbacks = await _unitOfWork.CustomerFeedbacks.Query()
                .Where(item => item.CustomerId == customer.Id && item.IsDelete == 0)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync(cancellationToken);

            var orderIds = feedbacks
                .Where(item => item.OrderId.HasValue)
                .Select(item => item.OrderId!.Value)
                .Distinct()
                .ToList();

            var orders = orderIds.Count == 0
                ? new Dictionary<long, Order>()
                : await _unitOfWork.Orders.Query()
                    .Where(item => orderIds.Contains(item.Id))
                    .ToDictionaryAsync(item => item.Id, cancellationToken);

            return feedbacks
                .Select(item => MapFeedback(item, orders.GetValueOrDefault(item.OrderId ?? 0)))
                .ToList();
        }

        public async Task<List<CustomerListItemVm>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken)
        {
            var normalizedSearch = string.IsNullOrWhiteSpace(searchTerm)
                ? null
                : searchTerm.Trim().ToLowerInvariant();

            var customers = await _unitOfWork.Customers.Query()
                .Where(item => item.IsDelete == 0)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync(cancellationToken);

            var customerIds = customers.Select(item => item.Id).ToList();
            var orders = customerIds.Count == 0
                ? new List<Order>()
                : await _unitOfWork.Orders.Query()
                    .Where(item => item.IsDelete == 0 && item.CustomerId.HasValue && customerIds.Contains(item.CustomerId.Value))
                    .ToListAsync(cancellationToken);

            var items = customers
                .Select(customer =>
                {
                    var customerOrders = orders
                        .Where(order => order.CustomerId == customer.Id)
                        .OrderByDescending(order => order.OrderDate)
                        .ToList();

                    return new CustomerListItemVm
                    {
                        Id = customer.Id,
                        UserId = customer.UserId,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        FullName = BuildFullName(customer.FirstName, customer.LastName),
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        City = customer.City,
                        ZipCode = customer.ZipCode,
                        Country = customer.Country,
                        IsActive = customer.IsActive,
                        TotalOrders = customerOrders.Count,
                        TotalSpent = customerOrders.Sum(order => order.TotalAmount),
                        LatestOrderStatus = NormalizeLatestOrderStatus(customerOrders.FirstOrDefault()?.Status),
                        LatestOrderDate = customerOrders.FirstOrDefault()?.OrderDate,
                        JoinedAt = customer.CreatedAt
                    };
                });

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                items = items.Where(item =>
                    item.FullName.ToLowerInvariant().Contains(normalizedSearch) ||
                    item.Email.ToLowerInvariant().Contains(normalizedSearch) ||
                    item.Phone.ToLowerInvariant().Contains(normalizedSearch));
            }

            return items.ToList();
        }

        public async Task<PaginatedResult<CustomerListItemVm>> GetPaginationAsync(PaginationRequest request, CancellationToken cancellationToken)
        {
            var safePageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var safePageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var normalizedSearch = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? null
                : request.SearchTerm.Trim().ToLowerInvariant();
            var query = _unitOfWork.Customers.Query()
                .Where(item => item.IsDelete == 0)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(item =>
                    ((item.FirstName ?? string.Empty) + " " + (item.LastName ?? string.Empty)).Trim().ToLower().Contains(normalizedSearch) ||
                    (item.Email ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                    (item.Phone ?? string.Empty).ToLower().Contains(normalizedSearch));
            }
            var totalRecords = await query.CountAsync(cancellationToken);
            var customers = await query
                .OrderByDescending(item => item.CreatedAt)
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync(cancellationToken);
            var customerIds = customers.Select(item => item.Id).ToList();
            var orders = customerIds.Count == 0
                ? new List<Order>()
                : await _unitOfWork.Orders.Query()
                    .Where(item => item.IsDelete == 0 && item.CustomerId.HasValue && customerIds.Contains(item.CustomerId.Value))
                    .ToListAsync(cancellationToken);
            var items = customers
                .Select(customer =>
                {
                    var customerOrders = orders
                        .Where(order => order.CustomerId == customer.Id)
                        .OrderByDescending(order => order.OrderDate)
                        .ToList();
                    return new CustomerListItemVm
                    {
                        Id = customer.Id,
                        UserId = customer.UserId,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        FullName = BuildFullName(customer.FirstName, customer.LastName),
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Address = customer.Address,
                        City = customer.City,
                        ZipCode = customer.ZipCode,
                        Country = customer.Country,
                        IsActive = customer.IsActive,
                        TotalOrders = customerOrders.Count,
                        TotalSpent = customerOrders.Sum(order => order.TotalAmount),
                        LatestOrderStatus = NormalizeLatestOrderStatus(customerOrders.FirstOrDefault()?.Status),
                        LatestOrderDate = customerOrders.FirstOrDefault()?.OrderDate,
                        JoinedAt = customer.CreatedAt
                    };
                })
                .ToList();
            var totalPages = totalRecords == 0 ? 0 : (int)Math.Ceiling(totalRecords / (double)safePageSize);
            return new PaginatedResult<CustomerListItemVm>
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
        private async Task<Customer> GetCurrentCustomerEntityAsync(CancellationToken cancellationToken)
        {
            var currentUser = await _workContext.CurrentUserAsync() ?? throw new Exception("Unauthorized request.");

            var customer = await _unitOfWork.Customers.Query()
                .FirstOrDefaultAsync(item => item.UserId == currentUser.Id && item.IsDelete == 0, cancellationToken);

            if (customer == null)
                throw new Exception("Customer profile not found.");

            return customer;
        }

        private async Task EnsureCustomerEmailAvailableAsync(string email, long? customerId, string? existingUserId, CancellationToken cancellationToken)
        {
            var duplicateCustomer = await _unitOfWork.Customers.AnyAsync(
                item => item.Email == email && item.IsDelete == 0 && (!customerId.HasValue || item.Id != customerId.Value),
                cancellationToken);

            if (duplicateCustomer)
                throw new Exception("A customer with this email already exists.");

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != existingUserId)
                throw new Exception("A user with this email already exists.");
        }

        private async Task EnsureCustomerPhoneAvailableAsync(string phone, long? customerId, CancellationToken cancellationToken)
        {
            var duplicateCustomer = await _unitOfWork.Customers.AnyAsync(
                item => item.Phone == phone && item.IsDelete == 0 && (!customerId.HasValue || item.Id != customerId.Value),
                cancellationToken);

            if (duplicateCustomer)
                throw new Exception("A customer with this phone number already exists.");
        }

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return;

            var result = await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                IsSystem = false
            });

            if (!result.Succeeded && !await _roleManager.RoleExistsAsync(roleName))
                throw new Exception(string.Join(" ", result.Errors.Select(item => item.Description)));
        }

        private static void NormalizeRegistrationModel(CustomerCheckoutRegistrationVm model)
        {
            if (model == null)
                throw new Exception("Invalid customer data.");

            model.FirstName = NormalizeRequiredText(model.FirstName, "First name is required.");
            model.LastName = NormalizeRequiredText(model.LastName, "Last name is required.");
            model.Email = NormalizeRequiredText(model.Email, "Email is required.");
            model.Phone = NormalizeRequiredText(model.Phone, "Phone is required.");
            model.Password = NormalizeRequiredText(model.Password, "Password is required.");
            model.Address = NormalizeRequiredText(model.Address, "Address is required.");
            model.City = NormalizeRequiredText(model.City, "City is required.");
            model.ZipCode = NormalizeRequiredText(model.ZipCode, "Zip code is required.");
            model.Country = NormalizeRequiredText(model.Country, "Country is required.");
        }

        private static void NormalizeAdminUpdateModel(UpdateCustomerAdminVm model)
        {
            if (model == null)
                throw new Exception("Invalid customer data.");

            model.FirstName = NormalizeRequiredText(model.FirstName, "First name is required.");
            model.LastName = NormalizeRequiredText(model.LastName, "Last name is required.");
            model.Email = NormalizeRequiredText(model.Email, "Email is required.");
            model.Phone = NormalizeRequiredText(model.Phone, "Phone is required.");
            model.Address = NormalizeRequiredText(model.Address, "Address is required.");
            model.City = NormalizeRequiredText(model.City, "City is required.");
            model.ZipCode = NormalizeRequiredText(model.ZipCode, "Zip code is required.");
            model.Country = NormalizeRequiredText(model.Country, "Country is required.");
        }
        private static void NormalizeProfileModel(UpdateCustomerProfileVm model)
        {
            if (model == null)
                throw new Exception("Invalid customer data.");

            model.FirstName = NormalizeRequiredText(model.FirstName, "First name is required.");
            model.LastName = NormalizeRequiredText(model.LastName, "Last name is required.");
            model.Phone = NormalizeRequiredText(model.Phone, "Phone is required.");
            model.Address = NormalizeRequiredText(model.Address, "Address is required.");
            model.City = NormalizeRequiredText(model.City, "City is required.");
            model.ZipCode = NormalizeRequiredText(model.ZipCode, "Zip code is required.");
            model.Country = NormalizeRequiredText(model.Country, "Country is required.");
        }

        private static void NormalizeFeedbackModel(CreateCustomerFeedbackVm model)
        {
            if (model == null)
                throw new Exception("Invalid feedback data.");

            if (model.Rating < 1 || model.Rating > 5)
                throw new Exception("Rating must be between 1 and 5.");

            model.Subject = NormalizeRequiredText(model.Subject, "Subject is required.");
            model.Message = NormalizeRequiredText(model.Message, "Message is required.");
        }

        private static string NormalizeRequiredText(string? value, string message)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new Exception(message);

            return normalized;
        }

        private static string BuildFullName(string firstName, string lastName) =>
            $"{firstName} {lastName}".Trim();

        private static string NormalizeLatestOrderStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "No orders";

            var normalizedStatus = status.Trim().ToLowerInvariant();
            return normalizedStatus == LegacyOrderStatusProcessing
                ? OrderStatusAccepted
                : status.Trim();
        }

        private static CustomerProfileVm MapProfile(Customer customer)
        {
            return new CustomerProfileVm
            {
                Id = customer.Id,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                FullName = BuildFullName(customer.FirstName, customer.LastName),
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                ZipCode = customer.ZipCode,
                Country = customer.Country,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            };
        }

        private static CustomerFeedbackVm MapFeedback(CustomerFeedback feedback, Order? order)
        {
            return new CustomerFeedbackVm
            {
                Id = feedback.Id,
                CustomerId = feedback.CustomerId,
                OrderId = feedback.OrderId,
                OrderNumber = order?.OrderNumber,
                Rating = feedback.Rating,
                Subject = feedback.Subject,
                Message = feedback.Message,
                Status = feedback.Status,
                CreatedAt = feedback.CreatedAt
            };
        }

        private static LoginResponseVM BuildLoginResponse(
            ApplicationUser user,
            IList<string> userRoles,
            string accessToken,
            string refreshToken)
        {
            return new LoginResponseVM
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new LoginUserVM
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    RoleNames = userRoles.ToList()
                },
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }
}




