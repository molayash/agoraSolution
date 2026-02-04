using CRM.Application.DTOs;
using CRM.Application.Services.Auth_Service;
using CRM.Application.Services.Menu_Permission_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MenuPermissionController : ControllerBase
    {
        private readonly IMenuPermissionService _menuPermissionService;
        private readonly ITokenService _tokenService;

        public MenuPermissionController(IMenuPermissionService menuPermissionService, ITokenService tokenService)
        {
            _menuPermissionService = menuPermissionService;
            _tokenService = tokenService;
        }

        [HttpPost("userMenu")]
        public async Task<IActionResult> AddUserMenuPermissions(
            [FromBody] AddMenuPermissionVM model,
            CancellationToken cancellationToken)
        {
            if (model == null)
                return BadRequest(new { Success = false, Message = "Invalid request." });

            var result = await _menuPermissionService.AddMenuPermissionByUserIdAsync(model, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("roleMenu")]
        public async Task<IActionResult> AddRoleMenuPermissions(
            [FromBody] AddRolePermissionVM model,
            CancellationToken cancellationToken)
        {
            if (model == null)
                return BadRequest(new { Success = false, Message = "Invalid request." });

            var result = await _menuPermissionService.AddPermissioneByRoleIdAsync(model, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getallrole")]
        public async Task<IActionResult> GetAllRole()
        {
            var data = await _tokenService.GetAllRolesAsync();
            if (data == null)
                return NotFound(new { message = "Menu not found." });

            return Ok(data);
        }



        [HttpGet("GetDefultRolePermissione")]
        public async Task<IActionResult> GetDefultRolePermissione([FromQuery] string? roleid = null)
        {
            var data = await _menuPermissionService.GetDefultPermissioneByRoleIdAsync(roleid);

            //if (data == null || !data.Any())
            //    return NotFound(new { message = "Menu not found." });

            return Ok(data);
        }

        [HttpPost("SaveDefaultRolePermissions")]
        public async Task<IActionResult> SaveDefaultRolePermissions(
            [FromBody] DefultRolePermissionVM model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseStatus
                {
                    Success = false,
                    Message = "Invalid request data."
                });
            }

            var result = await _menuPermissionService
                .AddDefultPermissioneByRoleIdAsync(model, cancellationToken);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



    }
}
