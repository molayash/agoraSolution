using CRM.Application.DTOs;
using CRM.Application.Services.ModuleMenu_Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public interface IMenuPermissionService
    {
        Task<ResponseStatus> AddMenuPermissionByUserIdAsync(AddMenuPermissionVM model, CancellationToken cancellationToken = default);
        Task<ResponseStatus> AddPermissioneByRoleIdAsync(AddRolePermissionVM model, CancellationToken cancellationToken = default);
        Task<ResponseStatus> AddDefultPermissioneByRoleIdAsync(DefultRolePermissionVM model, CancellationToken cancellationToken = default);
        Task<List<ModuleWiseMenuItemVm>> GetDefultPermissioneByRoleIdAsync(string? roleId,CancellationToken cancellationToken = default);
    }
}
