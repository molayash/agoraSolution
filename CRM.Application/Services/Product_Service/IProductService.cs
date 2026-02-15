using CRM.Application.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Product_Service
{
    public interface IProductService
    {
        Task<int> AddRecord(ProductViewModel model, CancellationToken ct);
        Task<ProductViewModel> GetAll(CancellationToken ct);
        Task<int> UpdateRecord(ProductViewModel model, CancellationToken ct);
        Task<bool> DeleteRecord(long id, CancellationToken ct);
        Task<ProductViewModel> GetRecord(long id, CancellationToken ct);
        Task<PaginatedResult<ProductViewModel>> GetPagination(PaginationRequest request, CancellationToken ct);
    }
}
