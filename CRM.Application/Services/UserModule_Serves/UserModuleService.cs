using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.UserModule_Serves;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Services
{
    public class UserModuleService : IUserModuleService
    {
        private readonly IWorkContext workContext;
        private readonly IUnitOfWork _unitOfWork;

        public UserModuleService(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            this.workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserModuleVM>> GetAllAsync()
        {
            return await _unitOfWork.UserModules.Query()
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

        public async Task<UserModuleVM?> GetByIdAsync(long id)
        {
            return await _unitOfWork.UserModules.Query()
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

        public async Task<long> CreateAsync(UserModuleVM model)
        {
            var user = await workContext.CurrentUserAsync();

            bool exists = await _unitOfWork.UserModules.AnyAsync(x =>
                x.IsDelete == 0 && x.ModuleName.ToLower() == model.ModuleName.ToLower());

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

            _unitOfWork.UserModules.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserModuleVM model)
        {
            var user = await workContext.CurrentUserAsync();
            var entity = await _unitOfWork.UserModules.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (entity == null) return false;

            bool exists = await _unitOfWork.UserModules.AnyAsync(x =>
                x.Id != model.Id && x.IsDelete == 0 && x.ModuleName.ToLower() == model.ModuleName.ToLower());

            if (exists)
                throw new Exception("Module name already exists.");

            entity.ModuleName = model.ModuleName.Trim();
            entity.SortOrder = model.SortOrder;
            entity.IsSubscribersModule = model.IsSubscribersModule;
            entity.IsActive = model.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _unitOfWork.UserModules.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0);

            if (entity == null) return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
