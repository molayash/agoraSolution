using CRM.Application.Interfaces.Repositories;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRM.Application.Services.Auth_Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("UserId", user.Id)
            };

            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserRefreshToken> GenerateRefreshTokenAsync(string userId)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var entity = new UserRefreshToken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _unitOfWork.UserRefreshTokens.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity;
        }

        public async Task<List<RoleVM>> GetAllRolesAsync()
        {
            return await _roleManager.Roles
                .Select(role => new RoleVM
                {
                    Id = role.Id,
                    Name = role.Name!,
                    IsSystem = role.IsSystem,
                })
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<UserRefreshToken> GetRefreshTokenAsync(string refreshtoken)
        {
            return await _unitOfWork.UserRefreshTokens.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.RefreshToken == refreshtoken);
        }

        public async Task<bool> RemoveRefreshTokenAsync(LogOutRequestVM model)
        {
            var tokens = await _unitOfWork.UserRefreshTokens.Query()
                .Where(h => h.UserId == model.UserId)
                .ToListAsync();

            if (tokens == null || !tokens.Any())
                return false;

            _unitOfWork.UserRefreshTokens.RemoveRange(tokens);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
