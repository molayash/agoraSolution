using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseVM> LoginAsync(LoginRequestVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
                throw new Exception("Invalid username or password.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid username or password.");

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
            var userRoles = await _userManager.GetRolesAsync(user);
            LoginUserVM userinfo = new LoginUserVM();
            userinfo.FullName = user.UserName;
            userinfo.Email = user.Email;
            userinfo.UserId = user.Id;
            userinfo.RoleNames = userRoles.ToList();

            return new LoginResponseVM
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.RefreshToken,
                User = userinfo,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
            };
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
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GetRefreshTokenAsync(model.RefreshToken);
            var userRoles = await _userManager.GetRolesAsync(user);
            LoginUserVM userinfo = new LoginUserVM();
            userinfo.FullName = user.UserName;
            userinfo.Email = user.Email;
            userinfo.UserId = user.Id;
            userinfo.RoleNames = userRoles.ToList();

            return new LoginResponseVM
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.RefreshToken,
                User = userinfo,
                AccessTokenExpiry = DateTime.UtcNow.AddMinutes(30)
            };
        }



    }
}
