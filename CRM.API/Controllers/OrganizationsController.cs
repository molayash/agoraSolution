using CRM.Application.Common.Pagination;
using CRM.Application.Services.Organization_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _service;

        public OrganizationsController(IOrganizationService service)
        {
            _service = service;
        }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpPost("getpaged")]
        public async Task<IActionResult> GetPaged([FromBody] PaginationRequest request)
        {
            var data = await _service.GetPagedAsync(request);
            return Ok(data);
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "Organization not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] OrganizationVM model)
        {
            try
            {
                var id = await _service.CreateAsync(model);
                return Ok(new { message = "Organization created successfully.", id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] OrganizationVM model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);
                if (!result)
                    return NotFound(new { message = "Organization not found." });

                return Ok(new { message = "Organization updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Organization not found." });

            return Ok(new { message = "Organization deleted successfully." });
        }
    }
}
