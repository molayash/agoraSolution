using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class Organization : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string OrganizationCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? TaxID { get; set; }
        public string? Website { get; set; }
        public string? PrimaryEmail { get; set; }
        public string? Description { get; set; }
        public DateTime? EnrollmentDate { get; set; }

    }
}
