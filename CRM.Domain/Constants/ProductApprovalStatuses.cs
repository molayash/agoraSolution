using System;
using System.Linq;

namespace CRM.Domain.Constants
{
    public static class ProductApprovalStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        public static readonly string[] All =
        {
            Pending,
            Approved,
            Rejected
        };

        public static string Normalize(string? value, string fallback = Approved)
        {
            var matched = All.FirstOrDefault(status =>
                string.Equals(status, value, StringComparison.OrdinalIgnoreCase));

            return matched ?? fallback;
        }
    }
}
