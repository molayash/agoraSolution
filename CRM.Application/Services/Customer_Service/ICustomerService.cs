using CRM.Application.Common.Pagination;
using CRM.Application.Services.Order_Service;
namespace CRM.Application.Services.Customer_Service
{
    public interface ICustomerService
    {
        Task<CustomerRegistrationResultVm> RegisterCheckoutAsync(CustomerCheckoutRegistrationVm model, CancellationToken cancellationToken);
        Task<CustomerProfileVm> GetCurrentProfileAsync(CancellationToken cancellationToken);
        Task<CustomerProfileVm> UpdateCurrentProfileAsync(UpdateCustomerProfileVm model, CancellationToken cancellationToken);
        Task<CustomerProfileVm> UpdateByAdminAsync(UpdateCustomerAdminVm model, CancellationToken cancellationToken);
        Task<List<OrderViewModel>> GetMyOrdersAsync(CancellationToken cancellationToken);
        Task<CustomerFeedbackVm> CreateFeedbackAsync(CreateCustomerFeedbackVm model, CancellationToken cancellationToken);
        Task<List<CustomerFeedbackVm>> GetMyFeedbacksAsync(CancellationToken cancellationToken);
        Task<List<CustomerListItemVm>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken);
        Task<PaginatedResult<CustomerListItemVm>> GetPaginationAsync(PaginationRequest request, CancellationToken cancellationToken);
    }
}

