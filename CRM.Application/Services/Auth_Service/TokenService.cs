using CRM.Domain.Entities.Auth;
using CRM.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly CrmDbContext _context;

        public TokenService(
            IConfiguration config,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
            CrmDbContext context)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ===================== ACCESS TOKEN =====================
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
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ===================== REFRESH TOKEN =====================
        public async Task<UserRefreshToken> GenerateRefreshTokenAsync(string userId)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var entity = new UserRefreshToken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            _context.UserRefreshTokens.Add(entity);
            await _context.SaveChangesAsync();

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
            var token = await _context.UserRefreshTokens.AsNoTracking().FirstOrDefaultAsync(h => h.RefreshToken == refreshtoken);
            return token;
        }

        public async Task<bool> RemoveRefreshTokenAsync(LogOutRequestVM model)
        {
            // 1. Find the token
            var token = await _context.UserRefreshTokens.Where(h => h.UserId == model.UserId).ToListAsync();

            if (token == null)
                return false; // token not found

            // 2. Remove it
            _context.UserRefreshTokens.RemoveRange(token);
            await _context.SaveChangesAsync();

            // 3. Return success
            return true;
        }

    }
}
