using System;
using System.Linq;

namespace CRM.Domain.Constants
{
    public static class VendorStatuses
    {
        public const string Pending = "Pending";
        public const string Partial = "Partial";
        public const string Active = "Active";
        public const string Cancel = "Cancel";

        public static readonly string[] All = { Pending, Partial, Active, Cancel };

        public static bool IsValid(string? status) =>
            All.Any(item => string.Equals(item, status, StringComparison.OrdinalIgnoreCase));

        public static string Normalize(string? status, string fallback = Pending)
        {
            var matchedStatus = All.FirstOrDefault(
                item => string.Equals(item, status, StringComparison.OrdinalIgnoreCase));

            return matchedStatus ?? fallback;
        }
    }
}
