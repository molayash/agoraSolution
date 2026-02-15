using CRM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Banner_Service
{
    public class BannerViewModel
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ButtonText { get; set; }
        public string? DiscountText { get; set; }
        public string? Link { get; set; }
        public int OrderNo { get; set; } = 0;
        public bool IsShow { get; set; } = true;

        public IQueryable<BannerViewModel>? BannerList { get; set; }
    }
}
