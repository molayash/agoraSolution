using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class Employee : EntityBase
    {

        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } 
        public string FullName { get; set; } = string.Empty;
        public string? WorkEmail { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Mobile { get; set; }
        public int EmploymentStatus { get; set; } 
        public string? Gender { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? DesignationID { get; set; }
        public long? DepartmentID { get; set; }
        public long? ManagerID { get; set; } // Reporting Line
        public string? UserID { get; set; }
        public string PhotoUrlWithPath { get; set; }= string.Empty;
    }
}
