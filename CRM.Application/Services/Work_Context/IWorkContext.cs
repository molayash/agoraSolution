using CRM.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Work_Context
{
    public interface IWorkContext
    {
        Task<ApplicationUser> GetCurrentUserAsync();
        Task<ApplicationUser> CurrentUserAsync();
        Task<bool> IsUserSignedIn();
    }
}
