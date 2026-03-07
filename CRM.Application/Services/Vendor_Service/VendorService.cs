using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.Services.Vendor_Service
{
    public class VendorService : IVendorService
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;

        public VendorService(IWorkContext workContext, CrmDbContext context)
        {
            _workContext = workContext;
            _context = context;
        }

        // ✅ ADD VENDOR
        public async Task<bool> Add(VendorVm model, CancellationToken cancellationToken)
        {
            var user = await _workContext.CurrentUserAsync();
            if (model == null) return false;

            // Prevent duplicate vendor by email
            var exists = await _context.Vendors
                .AnyAsync(x => x.Email == model.Email && x.IsDelete == 0, cancellationToken);

            if (exists)
                throw new Exception("Vendor with this email already exists.");

            var vendor = new Vendor
            {
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                CompanyName = model.CompanyName,
                CompanyWebsite = model.CompanyWebsite,
                Notes = model.Notes,
                IsActive = model.IsActive,
                IsDelete = 0,
                CreatedBy = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Vendors.AddAsync(vendor, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        // ✅ GET ALL
        public async Task<List<VendorVm>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Vendors
                .Where(x => x.IsDelete == 0)
                .OrderByDescending(x => x.Id)
                .Select(x => new VendorVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Address = x.Address,
                    CompanyName = x.CompanyName,
                    CompanyWebsite = x.CompanyWebsite,
                    Notes = x.Notes,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        // ✅ GET BY ID
        public async Task<VendorVm> GetById(long Id)
        {
            var vendor = await _context.Vendors
                .Where(x => x.Id == Id && x.IsDelete == 0)
                .Select(x => new VendorVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Address = x.Address,
                    CompanyName = x.CompanyName,
                    CompanyWebsite = x.CompanyWebsite,
                    Notes = x.Notes,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();

            if (vendor == null)
                throw new Exception("Vendor not found.");

            return vendor;
        }

        // ✅ UPDATE
        public async Task<bool> Update(VendorVm model)
        {
            var user = await _workContext.CurrentUserAsync();
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            // Check duplicate email for other vendors
            var exists = await _context.Vendors
                .AnyAsync(x => x.Email == model.Email && x.Id != model.Id && x.IsDelete == 0);

            if (exists)
                throw new Exception("Another vendor with the same email exists.");

            vendor.Name = model.Name;
            vendor.Phone = model.Phone;
            vendor.Email = model.Email;
            vendor.Address = model.Address;
            vendor.CompanyName = model.CompanyName;
            vendor.CompanyWebsite = model.CompanyWebsite;
            vendor.Notes = model.Notes;
            vendor.IsActive = model.IsActive;
            vendor.UpdatedBy = user.FullName;
            vendor.UpdatedAt = DateTime.UtcNow;

            _context.Vendors.Update(vendor);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ SOFT DELETE
        public async Task<bool> Delete(long Id)
        {
            var user = await _workContext.CurrentUserAsync();
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.Id == Id && x.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            vendor.IsDelete = 1;
            vendor.UpdatedBy = user.FullName;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
