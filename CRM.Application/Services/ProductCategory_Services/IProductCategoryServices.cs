using CRM.Application.Common.Result;

namespace CRM.Application.Services.ProductCategory_Services
{
    public interface IProductCategoryServices
    {
        Task<ProductCategoryViewModel> GetAllRecord(CancellationToken ct);
        Task<ProductCategoryViewModel> GetRecord(long id, CancellationToken ct);
        Task<ServiceResult> AddRecord(ProductCategoryViewModel model, CancellationToken ct);
        Task<ServiceResult> UpdateRecord(ProductCategoryViewModel model, CancellationToken ct);
        Task<ServiceResult> DeleteRecord(long id, CancellationToken ct);
    }
}
