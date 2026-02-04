using Microsoft.AspNetCore.Mvc;
using CRM.Application.Services.UserModule_Serves;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Authorization;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserModulesController : ControllerBase
    {
        private readonly IUserModuleService _service;

        public UserModulesController(IUserModuleService service)
        {
            _service = service;
        }

        // ========================= GET ALL =========================
        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // ========================= GET BY ID =========================
        [HttpGet("getbyid/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
                return NotFound(new { message = "User module not found." });

            return Ok(data);
        }

        // ========================= CREATE =========================
        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] UserModuleVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _service.CreateAsync(model);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id },
                    new { message = "User module created successfully.", id }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========================= UPDATE =========================
        [HttpPut("update")]
        public async Task<IActionResult> Update( [FromBody] UserModuleVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(model);

                if (!updated)
                    return NotFound(new { message = "User module not found." });

                return Ok(new { message = "User module updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ========================= DELETE =========================
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = "User module not found." });

            return Ok(new { message = "User module deleted successfully." });
        }
    }
}
