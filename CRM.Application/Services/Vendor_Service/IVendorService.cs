using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Vendor_Service
{
    public interface IVendorService
    {
        Task<bool> Add(VendorVm model, CancellationToken cancellationToken);
        Task<List<VendorVm>> GetAll(CancellationToken cancellationToken);
        Task<bool> Update(VendorVm model);
        Task<bool> Delete(long Id);
        Task<VendorVm> GetById(long Id);
    }
}
