using CRM.Application.Services.Auth_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    // GET: api/roles
    [HttpGet("getlist")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _rolesService.GetAllAsync();
        return Ok(roles);
    }

    // GET: api/roles/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var role = await _rolesService.GetByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(role);
    }

    // POST: api/roles
    [HttpPost("add")]
    public async Task<IActionResult> Create(RoleVM model)
    {
        var result = await _rolesService.CreateAsync(model);
        if (!result)
            return BadRequest("Role already exists");

        return Ok("Role created successfully");
    }

    // PUT: api/roles
    [HttpPut("update")]
    public async Task<IActionResult> Update(RoleVM model)
    {
        var result = await _rolesService.UpdateAsync(model);
        if (!result)
            return BadRequest("Role not found");

        return Ok("Role updated successfully");
    }
}
