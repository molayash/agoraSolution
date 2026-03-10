using CRM.Application.Services.UserModule_Serves;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("getbyid/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "User module not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] UserModuleVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _service.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id }, new { message = "User module created successfully.", id });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UserModuleVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(model);
            if (!updated)
                return NotFound(new { message = "User module not found." });

            return Ok(new { message = "User module updated successfully." });
        }

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
