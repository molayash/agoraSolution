using CRM.Application.Services.Employee_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // ✅ CREATE
        [HttpPost("add")]
        public async Task<IActionResult> Create([FromForm] EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _employeeService.AddAsync(model);

            return Ok(result);
        }

        // ✅ UPDATE

        [HttpPost("update")]
        public async Task<IActionResult> Update(long id, [FromForm] EmployeeViewModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch");

            var result = await _employeeService.UpdateAsync(model);

            if (result == null)
                return NotFound("Employee not found");

            return Ok(result);
        }

        // ✅ DELETE (Soft Delete)

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _employeeService.DeleteAsync(id);

            if (!deleted)
                return NotFound("Employee not found");

            return Ok("Employee deleted successfully");
        }

        // ✅ GET BY ID
        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // ✅ GET ALL
        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();

            return Ok(employees);
        }
    }
}
