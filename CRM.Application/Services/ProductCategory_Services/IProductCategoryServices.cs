using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ProductCategory_Services
{
    public interface IProductCategoryServices
    {
        Task<ProductCategoryViewModel> GetAllRecord(CancellationToken ct);
        Task<int> AddRecord(ProductCategoryViewModel model, CancellationToken ct);
        Task<int> UpdateRecord(ProductCategoryViewModel model, CancellationToken ct);
        Task<bool> DeleteRecord(long id, CancellationToken ct);
        Task<ProductCategoryViewModel> GetRecord(long id, CancellationToken ct);
    }
}
