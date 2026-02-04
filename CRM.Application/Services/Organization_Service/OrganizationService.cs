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

namespace CRM.Application.Services.Organization_Service
{
    public class OrganizationService : IOrganizationService
    {
        private readonly CrmDbContext _context;
        private readonly IPaginationService _paginationService;
        private readonly IWorkContext _workContext;

        public OrganizationService(
            CrmDbContext context,
            IPaginationService paginationService,
            IWorkContext workContext)
        {
            _context = context;
            _paginationService = paginationService;
            _workContext = workContext;
        }

        public async Task<IEnumerable<OrganizationVM>> GetAllAsync()
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.Name)
                .Select(x => new OrganizationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrganizationCode = x.OrganizationCode,
                    Address = x.Address,
                    Mobile = x.Mobile,
                    RegistrationNumber = x.RegistrationNumber,
                    TaxID = x.TaxID,
                    Website = x.Website,
                    PrimaryEmail = x.PrimaryEmail,
                    Description = x.Description,
                    EnrollmentDate = x.EnrollmentDate
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<OrganizationVM>> GetPagedAsync(PaginationRequest request)
        {
            var query = _context.Organizations
                .AsNoTracking()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.Name)
                .Select(x => new OrganizationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrganizationCode = x.OrganizationCode,
                    Address = x.Address,
                    Mobile = x.Mobile,
                    RegistrationNumber = x.RegistrationNumber,
                    TaxID = x.TaxID,
                    Website = x.Website,
                    PrimaryEmail = x.PrimaryEmail,
                    Description = x.Description,
                    EnrollmentDate = x.EnrollmentDate
                })
                .AsQueryable();

            return await _paginationService.PaginateAsync(query, request);
        }

        public async Task<OrganizationVM?> GetByIdAsync(long id)
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new OrganizationVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    OrganizationCode = x.OrganizationCode,
                    Address = x.Address,
                    Mobile = x.Mobile,
                    RegistrationNumber = x.RegistrationNumber,
                    TaxID = x.TaxID,
                    Website = x.Website,
                    PrimaryEmail = x.PrimaryEmail,
                    Description = x.Description,
                    EnrollmentDate = x.EnrollmentDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<long> CreateAsync(OrganizationVM model)
        {
            bool exists = await _context.Organizations
                .AnyAsync(x => x.OrganizationCode.ToLower() == model.OrganizationCode.ToLower() && x.IsDelete == 0);

            if (exists)
                throw new Exception("Organization code already exists.");

            var user = await _workContext.CurrentUserAsync();

            var entity = new Organization
            {
                Name = model.Name.Trim(),
                OrganizationCode = model.OrganizationCode.Trim(),
                Address = model.Address,
                Mobile = model.Mobile,
                RegistrationNumber = model.RegistrationNumber,
                TaxID = model.TaxID,
                Website = model.Website,
                PrimaryEmail = model.PrimaryEmail,
                Description = model.Description,
                EnrollmentDate = model.EnrollmentDate,
                IsDelete = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.FullName
            };

            _context.Organizations.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(OrganizationVM model)
        {
            var entity = await _context.Organizations
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (entity == null)
                return false;

            bool exists = await _context.Organizations
                .AnyAsync(x => x.Id != model.Id &&
                              x.OrganizationCode.ToLower() == model.OrganizationCode.ToLower() &&
                              x.IsDelete == 0);

            if (exists)
                throw new Exception("Organization code already exists.");

            var user = await _workContext.CurrentUserAsync();

            entity.Name = model.Name.Trim();
            entity.OrganizationCode = model.OrganizationCode.Trim();
            entity.Address = model.Address;
            entity.Mobile = model.Mobile;
            entity.RegistrationNumber = model.RegistrationNumber;
            entity.TaxID = model.TaxID;
            entity.Website = model.Website;
            entity.PrimaryEmail = model.PrimaryEmail;
            entity.Description = model.Description;
            entity.EnrollmentDate = model.EnrollmentDate;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = user.FullName;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Organizations
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
