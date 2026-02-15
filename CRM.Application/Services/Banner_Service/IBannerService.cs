using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Banner_Service
{
    public interface IBannerService
    {
        Task<BannerViewModel> GetAllRecord(CancellationToken ct);
        Task<BannerViewModel> GetRecord(long id, CancellationToken ct);
        Task<int> AddRecord(BannerViewModel model, CancellationToken ct);
        Task<int> UpdateRecord(BannerViewModel model, CancellationToken ct);
        Task<bool> DeleteRecord(long id, CancellationToken ct);
    }
}
