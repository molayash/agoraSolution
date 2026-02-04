using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities.Auth;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ModuleMenu_Service
{
    public class ModuleMenuService : IModuleMenuService
    {
        private readonly CrmDbContext _context;
        private readonly IWorkContext workContext;
        public ModuleMenuService(CrmDbContext context, IWorkContext workContext)
        {
            _context = context;
            this.workContext = workContext;
        }

        // ===================== GET ALL =====================
        public async Task<IEnumerable<ModuleMenuVM>> GetAllAsync()
        {
            return await _context.ModuleMenus
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .Join(
                    _context.UserModules,
                    menu => menu.UserModuleId,
                    module => module.Id,
                    (menu, module) => new ModuleMenuVM
                    {
                        Id = menu.Id,
                        UserModuleId = menu.UserModuleId,
                        MenuName = menu.MenuName,
                        MenuIcon = menu.MenuIcon,
                        HasChild = menu.HasChild,
                        ParentId = menu.ParentId,
                        Url = menu.Url,
                        IsVisible = menu.IsVisible,
                        IsEnabled = menu.IsEnabled,
                        SortOrder = menu.SortOrder,
                        UserModuleName = module.ModuleName,
                        ModuleSortOrder = module.SortOrder
                    }
                )
                .OrderBy(x => x.ModuleSortOrder)   // 1️⃣ first by module
                .ThenBy(x => x.SortOrder)          // 2️⃣ then by menu
                .ToListAsync();
        }


        // ===================== GET BY ID =====================
        public async Task<ModuleMenuVM?> GetByIdAsync(long id)
        {
            return await _context.ModuleMenus
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new ModuleMenuVM
                {
                    Id = x.Id,
                    UserModuleId = x.UserModuleId,
                    MenuName = x.MenuName,
                    MenuIcon = x.MenuIcon,
                    HasChild = x.HasChild,
                    ParentId = x.ParentId,
                    Url = x.Url,
                    IsVisible = x.IsVisible,
                    IsEnabled = x.IsEnabled,
                    SortOrder = x.SortOrder
                })
                .FirstOrDefaultAsync();
        }

        // ===================== CREATE =====================
        public async Task<long> CreateAsync(ModuleMenuVM model)
        {
            // 🔒 Duplicate menu validation (same module + parent)
            bool exists = await _context.ModuleMenus.AnyAsync(x =>
                x.IsDelete == 0 &&
                x.UserModuleId == model.UserModuleId &&
                x.ParentId == model.ParentId &&
                x.MenuName.ToLower() == model.MenuName.ToLower());

            if (exists)
                throw new Exception("Menu already exists under this module.");

            // 🔒 Parent validation
            //if (model.ParentId.HasValue)
            //{
            //    bool parentExists = await _context.ModuleMenus.AnyAsync(x =>
            //        x.Id == model.ParentId && x.IsDelete == 0);

            //    if (!parentExists)
            //        throw new Exception("Parent menu not found.");
            //}
            var user = await workContext.CurrentUserAsync();
            var entity = new ModuleMenu
            {
                UserModuleId = model.UserModuleId,
                MenuName = model.MenuName.Trim(),
                MenuIcon = model.MenuIcon,
                HasChild = model.HasChild,
                ParentId = model.ParentId,
                Url = model.Url,
                IsVisible = model.IsVisible,
                IsEnabled = model.IsEnabled,
                SortOrder = model.SortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.FullName,
                IsDelete = 0
            };

            _context.ModuleMenus.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ===================== UPDATE =====================
        public async Task<bool> UpdateAsync(ModuleMenuVM model)
        {
            var user = await workContext.CurrentUserAsync();
            var entity = await _context.ModuleMenus
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (entity == null)
                return false;

            bool exists = await _context.ModuleMenus.AnyAsync(x =>
                x.Id != model.Id &&
                x.IsDelete == 0 &&
                x.UserModuleId == model.UserModuleId &&
                x.ParentId == model.ParentId &&
                x.MenuName.ToLower() == model.MenuName.ToLower());

            if (exists)
                throw new Exception("Menu already exists.");

            entity.MenuName = model.MenuName.Trim();
            entity.MenuIcon = model.MenuIcon;
            entity.HasChild = model.HasChild;
            entity.ParentId = model.ParentId;
            entity.Url = model.Url;
            entity.IsVisible = model.IsVisible;
            entity.IsEnabled = model.IsEnabled;
            entity.SortOrder = model.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }

        // ===================== DELETE =====================
        public async Task<bool> DeleteAsync(long id)
        {
            var user = await workContext.CurrentUserAsync();

            var entity = await _context.ModuleMenus
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0);

            if (entity == null)
                return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }

     
    }
}
