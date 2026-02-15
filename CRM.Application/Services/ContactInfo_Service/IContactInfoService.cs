using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactInfo_Service
{
    public interface IContactInfoService
    {
        Task<ContactInfoVm> GetContactInfoAsync();
        Task<bool> UpdateContactInfoAsync(ContactInfoVm model);
    }
}
