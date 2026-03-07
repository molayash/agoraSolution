using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Vendor_Service
{
    public class VendorService : IVendorService
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;

        public VendorService(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Add(VendorVm model, CancellationToken cancellationToken)
        {
            var user = await _workContext.CurrentUserAsync();
            if (model == null) return false;

            var exists = await _unitOfWork.Vendors.AnyAsync(
                x => x.Email == model.Email && x.IsDelete == 0, cancellationToken);

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

            await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<List<VendorVm>> GetAll(CancellationToken cancellationToken)
        {
            return await _unitOfWork.Vendors.Query()
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

        public async Task<VendorVm> GetById(long Id)
        {
            var vendor = await _unitOfWork.Vendors.Query()
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

        public async Task<bool> Update(VendorVm model)
        {
            var user = await _workContext.CurrentUserAsync();
            var vendor = await _unitOfWork.Vendors.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            var exists = await _unitOfWork.Vendors.AnyAsync(
                x => x.Email == model.Email && x.Id != model.Id && x.IsDelete == 0);

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

            _unitOfWork.Vendors.Update(vendor);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(long Id)
        {
            var user = await _workContext.CurrentUserAsync();
            var vendor = await _unitOfWork.Vendors.Query()
                .FirstOrDefaultAsync(x => x.Id == Id && x.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            vendor.IsDelete = 1;
            vendor.UpdatedBy = user.FullName;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
