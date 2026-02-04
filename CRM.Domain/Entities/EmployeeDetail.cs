using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class EmployeeDetail : EntityBase
    {
        public long EmployeeID { get; set; }

        public virtual Employee? Employee { get; set; }

        [StringLength(150)]
        public string? FatherName { get; set; }

        [StringLength(150)]
        public string? MotherName { get; set; }

        [StringLength(150)]
        public string? SpouseName { get; set; }

        [StringLength(50)]
        public string? MaritalStatus { get; set; }

        public DateTime? MarriageDate { get; set; }

        [StringLength(50)]
        public string? BloodGroup { get; set; }

        [StringLength(150)]
        public string? Nationality { get; set; }

        [StringLength(100)]
        public string? NationalIDCard { get; set; }

        [StringLength(255)]
        public string? PresentAddress { get; set; }

        [StringLength(255)]
        public string? PermanentAddress { get; set; }

        public string? CurrentSalaryScale { get; set; }

        public string? PassportNo { get; set; }

        public string? DrivingLicenseNo { get; set; }

        public string? URLofCV { get; set; }

        public string? WhatsApp { get; set; }

        public string? LinkedIn { get; set; }

        public string? Twitter { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public string? FullCVDownloadURL { get; set; }


    }
}
