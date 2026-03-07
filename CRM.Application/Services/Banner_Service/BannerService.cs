using CRM.Application.Common.Result;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Banner_Service
{
    public class BannerService : IBannerService
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;

        public BannerService(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult> AddRecord(BannerViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var banner = new Banner
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                ButtonText = model.ButtonText,
                DiscountText = model.DiscountText,
                Link = model.Link,
                OrderNo = model.OrderNo,
                IsShow = model.IsShow,
                IsDelete = 0,
                CreatedBy = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Banners.AddAsync(banner, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Banner created successfully.");
        }

        public async Task<BannerViewModel> GetRecord(long id, CancellationToken ct)
        {
            var banner = await _unitOfWork.Banners.Query()
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new BannerViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    ButtonText = x.ButtonText,
                    DiscountText = x.DiscountText,
                    Link = x.Link,
                    OrderNo = x.OrderNo,
                    IsShow = x.IsShow
                })
                .FirstOrDefaultAsync(ct);

            if (banner == null)
                throw new Exception("Banner not found");

            return banner;
        }

        public async Task<ServiceResult> UpdateRecord(BannerViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var banner = await _unitOfWork.Banners.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (banner == null)
                return ServiceResult.NotFound("Banner not found.");

            banner.Title = model.Title.Trim();
            banner.Description = model.Description;
            banner.ImageUrl = model.ImageUrl;
            banner.ButtonText = model.ButtonText;
            banner.DiscountText = model.DiscountText;
            banner.Link = model.Link;
            banner.OrderNo = model.OrderNo;
            banner.IsShow = model.IsShow;
            banner.UpdatedBy = user.FullName;
            banner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Banner updated successfully.");
        }

        public async Task<ServiceResult> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var banner = await _unitOfWork.Banners.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (banner == null)
                return ServiceResult.NotFound("Banner not found.");

            banner.IsDelete = 1;
            banner.UpdatedBy = user.FullName;
            banner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Banner deleted successfully.");
        }

        public async Task<BannerViewModel> GetAllRecord(CancellationToken ct)
        {
            var model = new BannerViewModel();
            model.BannerList = (from t1 in _unitOfWork.Banners.Query()
                                where t1.IsDelete == 0
                                orderby t1.OrderNo
                                select new BannerViewModel
                                {
                                    Id = t1.Id,
                                    Title = t1.Title,
                                    Description = t1.Description,
                                    ImageUrl = t1.ImageUrl,
                                    ButtonText = t1.ButtonText,
                                    DiscountText = t1.DiscountText,
                                    Link = t1.Link,
                                    OrderNo = t1.OrderNo,
                                    IsShow = t1.IsShow
                                }).AsQueryable();
            return model;
        }
    }
}
