using CRM.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<UserRefreshToken> GenerateRefreshTokenAsync(string userId);
        Task<bool> RemoveRefreshTokenAsync(LogOutRequestVM model);
        Task<UserRefreshToken> GetRefreshTokenAsync(string refreshtoken);
        Task<List<RoleVM>> GetAllRolesAsync();  
    }
}
