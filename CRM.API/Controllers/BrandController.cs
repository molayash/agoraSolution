using CRM.Application.Services.Brand_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;

        public BrandController(IBrandService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var data = await _service.GetAll(cancellationToken);
            return Ok(data);
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetById(id);
            if (data == null)
                return NotFound(new { message = "Brand not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create(BrandVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.Add(model, cancellationToken);
            if (result)
                return Ok(new { message = "Brand created successfully." });

            return BadRequest(new { message = "Failed to create brand." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(BrandVm model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.Update(model);
            if (!result)
                return NotFound(new { message = "Brand not found." });

            return Ok(new { message = "Brand updated successfully." });
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.Delete(id);
            if (!result)
                return NotFound(new { message = "Brand not found." });

            return Ok(new { message = "Brand deleted successfully." });
        }
    }
}
