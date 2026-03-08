using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Services.Vendor_Service
{
    public interface IVendorService
    {
        Task<VendorCreateResultVm> Add(VendorVm model, CancellationToken cancellationToken);
        Task<List<VendorVm>> GetAll(CancellationToken cancellationToken);
        Task<bool> Update(VendorVm model);
        Task<bool> Delete(long Id);
        Task<VendorVm> GetById(long Id);
    }
}
