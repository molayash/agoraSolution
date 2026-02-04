using Microsoft.EntityFrameworkCore;
using CRM.Infrastructure;
using CRM.Application.Services.UserModule_Serves;
using CRM.Domain.Entities.Auth;
using CRM.Application.Services.Work_Context;

namespace Crm.Infrastructure.Services
{
    public class UserModuleService : IUserModuleService
    {
        private readonly IWorkContext workContext;
        private readonly CrmDbContext _context;

        public UserModuleService(IWorkContext workContext,CrmDbContext context)
        {
            this.workContext = workContext;
            _context = context;
        }

        // ========================= GET ALL =========================
        public async Task<IEnumerable<UserModuleVM>> GetAllAsync()
        {
    
            return await _context.UserModules
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.SortOrder)
                .Select(x => new UserModuleVM
                {
                    Id = x.Id,
                    ModuleName = x.ModuleName,
                    SortOrder = x.SortOrder,
                    IsSubscribersModule = x.IsSubscribersModule,
                    IsActive = x.IsActive,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        // ========================= GET BY ID =========================
        public async Task<UserModuleVM?> GetByIdAsync(long id)
        {
            return await _context.UserModules
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new UserModuleVM
                {
                    Id = x.Id,
                    ModuleName = x.ModuleName,
                    SortOrder = x.SortOrder,
                    IsSubscribersModule = x.IsSubscribersModule,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }

        // ========================= CREATE =========================
        public async Task<long> CreateAsync(UserModuleVM model)
        {
            var user = await workContext.CurrentUserAsync();
            // 🔒 Duplicate check
            bool exists = await _context.UserModules
                .AnyAsync(x =>
                    x.IsDelete == 0 &&
                    x.ModuleName.ToLower() == model.ModuleName.ToLower());

            if (exists)
                throw new Exception("Module name already exists.");

            var entity = new UserModule
            {
                ModuleName = model.ModuleName.Trim(),
                SortOrder = model.SortOrder,
                IsSubscribersModule = model.IsSubscribersModule,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.FullName,
                IsDelete = 0
            };

            _context.UserModules.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ========================= UPDATE =========================
        public async Task<bool> UpdateAsync(UserModuleVM model)
        {
            var user = await workContext.CurrentUserAsync();
            var entity = await _context.UserModules
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (entity == null)
                return false;

            // 🔒 Duplicate check (exclude self)
            bool exists = await _context.UserModules
                .AnyAsync(x =>
                    x.Id != model.Id &&
                    x.IsDelete == 0 &&
                    x.ModuleName.ToLower() == model.ModuleName.ToLower());

            if (exists)
                throw new Exception("Module name already exists.");

            entity.ModuleName = model.ModuleName.Trim();
            entity.SortOrder = model.SortOrder;
            entity.IsSubscribersModule = model.IsSubscribersModule;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }

        // ========================= DELETE =========================
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.UserModules
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0);

            if (entity == null)
                return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
