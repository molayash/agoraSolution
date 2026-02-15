using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactMessage_Service
{
    public interface IContactMessageService
    {
        Task<List<ContactMessageVm>> GetListAsync();
        Task<bool> AddAsync(ContactMessageVm model);
        Task<bool> MarkAsSeenAsync(long id);
        Task<bool> DeleteAsync(long id);
    }
}
