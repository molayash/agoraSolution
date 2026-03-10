using CRM.Application.Services.ModuleMenu_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ModuleMenusController : ControllerBase
    {
        private readonly IModuleMenuService _service;

        public ModuleMenusController(IModuleMenuService service)
        {
            _service = service;
        }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "Menu not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create(ModuleMenuVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _service.CreateAsync(model);
            return Ok(new { message = "Menu created successfully.", id });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ModuleMenuVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(model);
            if (!result)
                return NotFound(new { message = "Menu not found." });

            return Ok(new { message = "Menu updated successfully." });
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Menu not found." });

            return Ok(new { message = "Menu deleted successfully." });
        }
    }
}
