using CRM.Application.Common.Result;

namespace CRM.Application.Services.ProductSubCategory_Service
{
    public interface IProductSubCategoryService
    {
        Task<ProductSubCategoryViewModel> GetAll(CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetRecord(long id, CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetProductTypeWiseList(long id, CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct);
        Task<ServiceResult> AddRecord(ProductSubCategoryViewModel model, CancellationToken ct);
        Task<ServiceResult> UpdateRecord(ProductSubCategoryViewModel model, CancellationToken ct);
        Task<ServiceResult> DeleteRecord(long id, CancellationToken ct);
    }
}
