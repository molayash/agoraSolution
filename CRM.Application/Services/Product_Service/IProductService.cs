using CRM.Application.Common.Pagination;
using CRM.Application.Common.Result;

namespace CRM.Application.Services.Product_Service
{
    public interface IProductService
    {
        Task<ProductViewModel> GetAll(CancellationToken ct);
        Task<ProductViewModel> GetRecord(long id, CancellationToken ct);
        Task<PaginatedResult<ProductViewModel>> GetPagination(PaginationRequest request, CancellationToken ct);
        Task<ServiceResult> AddRecord(ProductViewModel model, CancellationToken ct);
        Task<ServiceResult> UpdateRecord(ProductViewModel model, CancellationToken ct);
        Task<ServiceResult> DeleteRecord(long id, CancellationToken ct);
    }
}
