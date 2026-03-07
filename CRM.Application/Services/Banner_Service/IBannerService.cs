using CRM.Application.Common.Result;

namespace CRM.Application.Services.Banner_Service
{
    public interface IBannerService
    {
        Task<BannerViewModel> GetAllRecord(CancellationToken ct);
        Task<BannerViewModel> GetRecord(long id, CancellationToken ct);
        Task<ServiceResult> AddRecord(BannerViewModel model, CancellationToken ct);
        Task<ServiceResult> UpdateRecord(BannerViewModel model, CancellationToken ct);
        Task<ServiceResult> DeleteRecord(long id, CancellationToken ct);
    }
}
