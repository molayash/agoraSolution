using CRM.Application.DTOs;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities.Auth;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class MenuPermissionService : IMenuPermissionService
    {
        private readonly CrmDbContext _context;
        private readonly IWorkContext workContext;
        public MenuPermissionService(CrmDbContext context, IWorkContext workContext)
        {
            _context = context;
            this.workContext = workContext;
        }

  
        public async Task<ResponseStatus> AddMenuPermissionByUserIdAsync(AddMenuPermissionVM model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.UserId))
                {
                    return new ResponseStatus
                    {
                        Id = 0,
                        Success = false,
                        Message = "Invalid user information."
                    };

                }

                if (model.Permissions == null || !model.Permissions.Any())
                {
                    return new ResponseStatus
                    {
                        Id = 0,
                        Success = false,
                        Message = "No menu permissions provided."
                    };

                }

                var menuIds = model.Permissions
                                   .Where(x => x.MenuId > 0)
                                   .Select(x => x.MenuId)
                                   .ToList();

                var existingPermissions = await _context.MenuPermissions
                    .Where(x => x.UserId == model.UserId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.Permissions)
                {
                    var existing = existingPermissions
                        .FirstOrDefault(x => x.MenuId == permission.MenuId);

                    bool hasAnyPermission =
                        (permission.CanAdd ?? false) ||
                        (permission.CanView ?? false) ||
                        (permission.CanEdit ?? false) ||
                        (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                        {
                            _context.MenuPermissions.Remove(existing);
                        }
                        else
                        {
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;

                            _context.MenuPermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission)
                            continue;

                        await _context.MenuPermissions.AddAsync(
             new MenuPermission
             {
                 UserId = model.UserId,
                 MenuId = permission.MenuId,
                 CanAdd = permission.CanAdd,
                 CanView = permission.CanView,
                 CanEdit = permission.CanEdit,
                 CanDelete = permission.CanDelete
             },
             cancellationToken
         );

                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                return new ResponseStatus
                {
                    Id = 0,
                    Success = true,
                    Message = "Menu permissions updated successfully."
                };

            }
            catch (Exception ex)
            {
                return new ResponseStatus
                {
                    Id = 0,
                    Success = true,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        public async Task<ResponseStatus> AddPermissioneByRoleIdAsync(
        AddRolePermissionVM model,
        CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.UserId))
                return new ResponseStatus
                {
                    Success = false,
                    Message = "Invalid role/user information."
                };

            if (model.models == null || !model.models.Any())
                return new ResponseStatus
                {
                    Success = false,
                    Message = "No role permissions provided."
                };

            try
            {
                // Get list of MenuIds from the request
                var menuIds = model.models
                    .Where(x => x.MenuId > 0)
                    .Select(x => x.MenuId)
                    .ToList();

                // Fetch existing role permissions from the DB
                var existingPermissions = await _context.MenuRolePermissions
                    .Where(x => x.RoleId == model.UserId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.models)
                {
                    // Check if this permission already exists
                    var existing = existingPermissions
                        .FirstOrDefault(x => x.MenuId == permission.MenuId);

                    bool hasAnyPermission =
                        (permission.CanAdd ?? false) ||
                        (permission.CanView ?? false) ||
                        (permission.CanEdit ?? false) ||
                        (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                        {
                            // Remove permission if all flags are false
                            _context.MenuRolePermissions.Remove(existing);
                        }
                        else
                        {
                            // Update existing permission
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;
                            _context.MenuRolePermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission)
                            continue;

                        // Add new permission
                        await _context.MenuRolePermissions.AddAsync(
                            new MenuRolePermissions
                            {
                                RoleId = permission.RoleId,
                                MenuId = permission.MenuId,

                                CanAdd = permission.CanAdd,
                                CanView = permission.CanView,
                                CanEdit = permission.CanEdit,
                                CanDelete = permission.CanDelete
                            },
                            cancellationToken
                        );
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return new ResponseStatus
                {
                    Success = true,
                    Message = "Role permissions updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseStatus
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<List<ModuleWiseMenuItemVm>> GetDefultPermissioneByRoleIdAsync(
            string? roleId,
            CancellationToken cancellationToken = default)
        {
            var data = await (
                from m in _context.ModuleMenus
                join um in _context.UserModules
                    on m.UserModuleId equals um.Id

                join mp in _context.DefultMenuRolePermissions
                        .Where(x => roleId != null && x.RoleId == roleId)
                    on m.Id equals mp.MenuId into mpGroup
                from mp in mpGroup.DefaultIfEmpty() // LEFT JOIN

                //where um.IsSubscribersModule
                //      && !string.IsNullOrEmpty(m.Url)

                orderby um.SortOrder, m.SortOrder

                select new
                {
                    ModuleId = um.Id,
                    um.ModuleName,
                    um.SortOrder,
                    Menu = new RolePermissionVM
                    {
                        Id = mp != null ? mp.Id : null,
                        RoleId = roleId,
                        MenuId = m.Id,
                        MenuName = m.MenuName,
                        CanAdd = mp != null ? mp.CanAdd : false,
                        CanEdit = mp != null ? mp.CanEdit : false,
                        CanDelete = mp != null ? mp.CanDelete : false,
                        CanView = mp != null ? mp.CanView : false
                    }
                }
            ).ToListAsync(cancellationToken);

            var result = data
                .GroupBy(x => new { x.ModuleId, x.ModuleName, x.SortOrder })
                .Select(g => new ModuleWiseMenuItemVm
                {
                    Id = g.Key.ModuleId,
                    ModuleName = g.Key.ModuleName,
                    SortOrder = g.Key.SortOrder,
                    RolePermissions = g.Select(x => x.Menu).ToList()
                })
                .OrderBy(x => x.SortOrder)
                .ToList();

            return result;
        }


        public async Task<ResponseStatus> AddDefultPermissioneByRoleIdAsync(
            DefultRolePermissionVM model,
            CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RoleId))
                return new ResponseStatus
                {
                    Success = false,
                    Message = "Invalid role information."
                };

            if (model.models == null || !model.models.Any())
                return new ResponseStatus
                {
                    Success = false,
                    Message = "No role permissions provided."
                };

            try
            {
                // Get list of MenuIds from the request
                var menuIds = model.models
                    .Where(x => x.MenuId > 0)
                    .Select(x => x.MenuId)
                    .ToList();

                // Fetch existing role permissions from the DB
                var existingPermissions = await _context.DefultMenuRolePermissions
                    .Where(x => x.RoleId == model.RoleId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.models)
                {
                    // Check if this permission already exists
                    var existing = existingPermissions
                        .FirstOrDefault(x => x.MenuId == permission.MenuId);

                    bool hasAnyPermission =
                        (permission.CanAdd ?? false) ||
                        (permission.CanView ?? false) ||
                        (permission.CanEdit ?? false) ||
                        (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                        {
                            // Remove permission if all flags are false
                            _context.DefultMenuRolePermissions.Remove(existing);
                        }
                        else
                        {
                            // Update existing permission
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;
                            _context.DefultMenuRolePermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission)
                            continue;

                        // Add new permission
                        await _context.DefultMenuRolePermissions.AddAsync(
                            new DefultMenuRolePermissions
                            {
                                RoleId = permission.RoleId,
                                MenuId = permission.MenuId,

                                CanAdd = permission.CanAdd,
                                CanView = permission.CanView,
                                CanEdit = permission.CanEdit,
                                CanDelete = permission.CanDelete
                            },
                            cancellationToken
                        );
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return new ResponseStatus
                {
                    Success = true,
                    Message = "Role permissions updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseStatus
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }







    }
}

