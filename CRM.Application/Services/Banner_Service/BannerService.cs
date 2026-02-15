using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Banner_Service
{
    public class BannerService : IBannerService
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;

        public BannerService(IWorkContext workContext, CrmDbContext context)
        {
            _workContext = workContext;
            _context = context;
        }

        public async Task<int> AddRecord(BannerViewModel model, CancellationToken ct)
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

            await _context.Banner.AddAsync(banner, ct);
            await _context.SaveChangesAsync(ct);

            return 2; // Success
        }

        public async Task<BannerViewModel> GetRecord(long id, CancellationToken ct)
        {
            var banner = await _context.Banner
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

        public async Task<int> UpdateRecord(BannerViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var banner = await _context.Banner
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (banner == null)
                return 0; // Not found

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

            await _context.SaveChangesAsync(ct);

            return 2; // Updated
        }

        public async Task<bool> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var banner = await _context.Banner
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (banner == null)
                return false;

            banner.IsDelete = 1;
            banner.UpdatedBy = user.FullName;
            banner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<BannerViewModel> GetAllRecord(CancellationToken ct)
        {
            BannerViewModel model = new BannerViewModel();
            model.BannerList = await Task.Run(() => (from t1 in _context.Banner
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
                                                     }).AsQueryable());
            return model;
        }
    }
}
