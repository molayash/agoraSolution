using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class Banner : EntityBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ButtonText { get; set; }
        public string DiscountText { get; set; }
        public string Link { get; set; }
        public int OrderNo { get; set; }
        public bool IsShow { get; set; }
    }
}
