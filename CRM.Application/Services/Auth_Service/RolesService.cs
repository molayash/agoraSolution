using CRM.Application.Services.Auth_Service;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;

public class RolesService : IRolesService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RolesService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<RoleVM>> GetAllAsync()
    {
        return _roleManager.Roles.Select(r => new RoleVM
        {
            Id = r.Id,
            Name = r.Name!,
            IsSystem = r.IsSystem
        }).ToList();
    }

    public async Task<RoleVM?> GetByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return null;

        return new RoleVM
        {
            Id = role.Id,
            Name = role.Name!,
            IsSystem = role.IsSystem
        };
    }

    public async Task<bool> CreateAsync(RoleVM model)
    {
        try
        {
            if (await _roleManager.RoleExistsAsync(model.Name))
                return false;

            var role = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                IsSystem = model.IsSystem
            };

            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            return false;   
        }

    }

    public async Task<bool> UpdateAsync(RoleVM model)
    {
        var role = await _roleManager.FindByIdAsync(model.Id);
        if (role == null) return false;

        // 🔒 Optional safety: block system role edits
        if (role.IsSystem)
            return false;

        role.Name = model.Name;
        role.IsSystem = model.IsSystem;

        var result = await _roleManager.UpdateAsync(role);
        return result.Succeeded;
    }
}
