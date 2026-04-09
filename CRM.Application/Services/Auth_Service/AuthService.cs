using CRM.Application.Interfaces.Repositories;
using CRM.Domain.Constants;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Auth_Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResponseVM> LoginAsync(LoginRequestVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
                throw new Exception("Invalid username or password.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid username or password.");

            var userRoles = await _userManager.GetRolesAsync(user);
            await EnsureVendorAccessAsync(user.Id, userRoles);

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);

            return BuildLoginResponse(user, userRoles, accessToken, refreshToken.RefreshToken);
        }

        public async Task<bool> RemoveRefreshTokenAsync(LogOutRequestVM model)
        {
            return await _tokenService.RemoveRefreshTokenAsync(model);
        }

        public async Task<LoginResponseVM> RefreshTokenAsync(LogOutRequestVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                throw new Exception("Invalid user.");

            var userRoles = await _userManager.GetRolesAsync(user);
            await EnsureVendorAccessAsync(user.Id, userRoles);

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GetRefreshTokenAsync(model.RefreshToken);

            return BuildLoginResponse(user, userRoles, accessToken, refreshToken.RefreshToken);
        }

        private async Task EnsureVendorAccessAsync(string userId, IList<string> userRoles)
        {
            if (!userRoles.Any(role => role.Equals("Vendor", StringComparison.OrdinalIgnoreCase)))
                return;

            var vendor = await _unitOfWork.Vendors.Query()
                .Where(item => item.UserId == userId && item.IsDelete == 0)
                .FirstOrDefaultAsync();

            if (vendor == null)
                throw new Exception("Vendor account not found.");

            if (!vendor.IsActive || !string.Equals(vendor.Status, VendorStatuses.Active, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Your vendor account is currently {VendorStatuses.Normalize(vendor.Status).ToLowerInvariant()}.");
        }

        private static LoginResponseVM BuildLoginResponse(
            ApplicationUser user,
            IList<string> userRoles,
            string accessToken,
            string refreshToken)
        {
            var userinfo = new LoginUserVM
            {
                FullName = user.UserName,
                Email = user.Email,
                UserId = user.Id,
                RoleNames = userRoles.ToList()
            };

            return new LoginResponseVM
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userinfo,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }
}
