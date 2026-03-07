using CRM.Application.DTOs;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class MenuPermissionService : IMenuPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext workContext;

        public MenuPermissionService(IUnitOfWork unitOfWork, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            this.workContext = workContext;
        }

        public async Task<ResponseStatus> AddMenuPermissionByUserIdAsync(AddMenuPermissionVM model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.UserId))
                    return new ResponseStatus { Id = 0, Success = false, Message = "Invalid user information." };

                if (model.Permissions == null || !model.Permissions.Any())
                    return new ResponseStatus { Id = 0, Success = false, Message = "No menu permissions provided." };

                var menuIds = model.Permissions.Where(x => x.MenuId > 0).Select(x => x.MenuId).ToList();

                var existingPermissions = await _unitOfWork.MenuPermissions.Query()
                    .Where(x => x.UserId == model.UserId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.Permissions)
                {
                    var existing = existingPermissions.FirstOrDefault(x => x.MenuId == permission.MenuId);
                    bool hasAnyPermission = (permission.CanAdd ?? false) || (permission.CanView ?? false)
                                         || (permission.CanEdit ?? false) || (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                            _unitOfWork.MenuPermissions.Remove(existing);
                        else
                        {
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;
                            _unitOfWork.MenuPermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission) continue;

                        await _unitOfWork.MenuPermissions.AddAsync(new MenuPermission
                        {
                            UserId = model.UserId,
                            MenuId = permission.MenuId,
                            CanAdd = permission.CanAdd,
                            CanView = permission.CanView,
                            CanEdit = permission.CanEdit,
                            CanDelete = permission.CanDelete
                        }, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new ResponseStatus { Id = 0, Success = true, Message = "Menu permissions updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { Id = 0, Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ResponseStatus> AddPermissioneByRoleIdAsync(AddRolePermissionVM model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.UserId))
                return new ResponseStatus { Success = false, Message = "Invalid role/user information." };

            if (model.models == null || !model.models.Any())
                return new ResponseStatus { Success = false, Message = "No role permissions provided." };

            try
            {
                var menuIds = model.models.Where(x => x.MenuId > 0).Select(x => x.MenuId).ToList();

                var existingPermissions = await _unitOfWork.MenuRolePermissions.Query()
                    .Where(x => x.RoleId == model.UserId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.models)
                {
                    var existing = existingPermissions.FirstOrDefault(x => x.MenuId == permission.MenuId);
                    bool hasAnyPermission = (permission.CanAdd ?? false) || (permission.CanView ?? false)
                                         || (permission.CanEdit ?? false) || (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                            _unitOfWork.MenuRolePermissions.Remove(existing);
                        else
                        {
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;
                            _unitOfWork.MenuRolePermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission) continue;

                        await _unitOfWork.MenuRolePermissions.AddAsync(new MenuRolePermissions
                        {
                            RoleId = permission.RoleId,
                            MenuId = permission.MenuId,
                            CanAdd = permission.CanAdd,
                            CanView = permission.CanView,
                            CanEdit = permission.CanEdit,
                            CanDelete = permission.CanDelete
                        }, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new ResponseStatus { Success = true, Message = "Role permissions updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<List<ModuleWiseMenuItemVm>> GetDefultPermissioneByRoleIdAsync(string? roleId, CancellationToken cancellationToken = default)
        {
            var data = await (
                from m in _unitOfWork.ModuleMenus.Query()
                join um in _unitOfWork.UserModules.Query() on m.UserModuleId equals um.Id
                join mp in _unitOfWork.DefaultMenuRolePermissions.Query()
                        .Where(x => roleId != null && x.RoleId == roleId)
                    on m.Id equals mp.MenuId into mpGroup
                from mp in mpGroup.DefaultIfEmpty()
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

            return data
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
        }

        public async Task<ResponseStatus> AddDefultPermissioneByRoleIdAsync(DefultRolePermissionVM model, CancellationToken cancellationToken = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RoleId))
                return new ResponseStatus { Success = false, Message = "Invalid role information." };

            if (model.models == null || !model.models.Any())
                return new ResponseStatus { Success = false, Message = "No role permissions provided." };

            try
            {
                var menuIds = model.models.Where(x => x.MenuId > 0).Select(x => x.MenuId).ToList();

                var existingPermissions = await _unitOfWork.DefaultMenuRolePermissions.Query()
                    .Where(x => x.RoleId == model.RoleId && menuIds.Contains(x.MenuId))
                    .ToListAsync(cancellationToken);

                foreach (var permission in model.models)
                {
                    var existing = existingPermissions.FirstOrDefault(x => x.MenuId == permission.MenuId);
                    bool hasAnyPermission = (permission.CanAdd ?? false) || (permission.CanView ?? false)
                                         || (permission.CanEdit ?? false) || (permission.CanDelete ?? false);

                    if (existing != null)
                    {
                        if (!hasAnyPermission)
                            _unitOfWork.DefaultMenuRolePermissions.Remove(existing);
                        else
                        {
                            existing.CanAdd = permission.CanAdd;
                            existing.CanView = permission.CanView;
                            existing.CanEdit = permission.CanEdit;
                            existing.CanDelete = permission.CanDelete;
                            _unitOfWork.DefaultMenuRolePermissions.Update(existing);
                        }
                    }
                    else
                    {
                        if (!hasAnyPermission) continue;

                        await _unitOfWork.DefaultMenuRolePermissions.AddAsync(new DefultMenuRolePermissions
                        {
                            RoleId = permission.RoleId,
                            MenuId = permission.MenuId,
                            CanAdd = permission.CanAdd,
                            CanView = permission.CanView,
                            CanEdit = permission.CanEdit,
                            CanDelete = permission.CanDelete
                        }, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return new ResponseStatus { Success = true, Message = "Role permissions updated successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseStatus { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
