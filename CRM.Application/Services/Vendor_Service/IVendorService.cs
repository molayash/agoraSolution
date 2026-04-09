using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Services.Vendor_Service
{
    public interface IVendorService
    {
        Task<VendorCreateResultVm> Add(VendorVm model, CancellationToken cancellationToken);
        Task<long> SubmitRegistrationRequest(VendorRegistrationRequestVm model, CancellationToken cancellationToken);
        Task<List<VendorVm>> GetAll(CancellationToken cancellationToken);
        Task<VendorCreateResultVm> Update(VendorVm model, CancellationToken cancellationToken);
        Task<bool> Delete(long Id);
        Task<VendorVm> GetById(long Id);
    }
}
