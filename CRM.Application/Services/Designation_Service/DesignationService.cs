using CRM.Application.Common.Pagination;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Designation_Service
{
    public class DesignationService : IDesignationService
    {
        private readonly CrmDbContext _context;
        private readonly IPaginationService _paginationService;
        private readonly IWorkContext _workContext;

        public DesignationService(
            CrmDbContext context,
            IPaginationService paginationService,
            IWorkContext workContext)
        {
            _context = context;
            _paginationService = paginationService;
            _workContext = workContext;
        }

        // ===================== GET ALL =====================
        public async Task<IEnumerable<DesignationVM>> GetAllAsync()
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.SortOrder)
                .Select(x => new DesignationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    SortOrder = x.SortOrder
                })
                .ToListAsync();
        }

        // ===================== PAGINATED =====================
        public async Task<PaginatedResult<DesignationVM>> GetPagedAsync(PaginationRequest request)
        {
            var query = _context.Designations
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.SortOrder)
                .Select(x => new DesignationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    SortOrder = x.SortOrder
                })
                .AsQueryable();

            return await _paginationService.PaginateAsync(query, request);
        }

        // ===================== GET BY ID =====================
        public async Task<DesignationVM?> GetByIdAsync(long id)
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new DesignationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description,
                    SortOrder = x.SortOrder
                })
                .FirstOrDefaultAsync();
        }

        // ===================== CREATE =====================
        public async Task<long> CreateAsync(DesignationVM model)
        {
            // Check unique code
            bool exists = await _context.Designations
                .AnyAsync(x => x.Code.ToLower() == model.Code.ToLower() && x.IsDelete == 0);

            if (exists)
                throw new Exception("Designation code already exists.");

            var user = await _workContext.CurrentUserAsync();

            var entity = new Designation
            {
                Name = model.Name.Trim(),
                Code = model.Code.Trim(),
                Description = model.Description,
                SortOrder = model.SortOrder,
                IsDelete = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.FullName
            };

            _context.Designations.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        // ===================== UPDATE =====================
        public async Task<bool> UpdateAsync(DesignationVM model)
        {
            var entity = await _context.Designations
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (entity == null)
                return false;

            // Check unique code (exclude self)
            bool exists = await _context.Designations
                .AnyAsync(x => x.Id != model.Id && x.Code.ToLower() == model.Code.ToLower() && x.IsDelete == 0);

            if (exists)
                throw new Exception("Designation code already exists.");

            var user = await _workContext.CurrentUserAsync();

            entity.Name = model.Name.Trim();
            entity.Code = model.Code.Trim();
            entity.Description = model.Description;
            entity.SortOrder = model.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }

        // ===================== DELETE =====================
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Designations
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0);

            if (entity == null)
                return false;

            var user = await _workContext.CurrentUserAsync();

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
