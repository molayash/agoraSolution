using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ProductSubCategory_Service
{
    public interface IProductSubCategoryService
    {
        Task<int> AddRecord(ProductSubCategoryViewModel model, CancellationToken ct);
        Task<int> UpdateRecord(ProductSubCategoryViewModel model, CancellationToken ct);
        Task<bool> DeleteRecord(long id, CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetRecord(long id, CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetProductTypeWiseList(long id, CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetAll(CancellationToken ct);
        Task<ProductSubCategoryViewModel> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct);
    }
}
