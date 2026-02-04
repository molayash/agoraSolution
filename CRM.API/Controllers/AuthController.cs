using CRM.Application.Services.Auth_Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace CRM.WebAPI.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogOutRequestVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _authService.RemoveRefreshTokenAsync(model);
            if (!success)
                return BadRequest(new { message = "Invalid refresh token" });

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshTokenAsync(LogOutRequestVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RefreshTokenAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }



    }
}
