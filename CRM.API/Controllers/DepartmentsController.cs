using CRM.Application.Common.Pagination;
using CRM.Application.Services.Department_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _service;

        public DepartmentsController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpPost("getpaged")]
        public async Task<IActionResult> GetPaged([FromBody] PaginationRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid pagination request." });

            var data = await _service.GetPagedAsync(request);
            return Ok(data);
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound(new { message = "Department not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] DepartmentVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var id = await _service.CreateAsync(model);
                return Ok(new { message = "Department created successfully.", id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] DepartmentVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateAsync(model);
                if (!result)
                    return NotFound(new { message = "Department not found." });

                return Ok(new { message = "Department updated successfully." });
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
                return NotFound(new { message = "Department not found." });

            return Ok(new { message = "Department deleted successfully." });
        }
    }
}