using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Employee_Service
{
    public class EmployeeViewModel
    {
        public long Id { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; }
        public string? WorkEmail { get; set; }
        public string? PersonalEmail { get; set; }
        public string? Mobile { get; set; }
        public int EmploymentStatus { get; set; }
        public string? Gender { get; set; }

        public DateTime? JoiningDate { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public long? DesignationID { get; set; }
        public string? DesignationName { get; set; }
        public long? DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public long? ManagerID { get; set; }
        public string? ManagerName { get; set; }
        public string? UserID { get; set; }
        public IFormFile ProfileImage { get; set; }
        public string? PhotoUrlWithPath { get; set; }
    }

}
