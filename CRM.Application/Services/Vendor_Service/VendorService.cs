using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CRM.Application.Services.Vendor_Service
{
    public class VendorService : IVendorService
    {
        private const string VendorRoleName = "Vendor";

        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public VendorService(
            IWorkContext workContext,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<VendorCreateResultVm> Add(VendorVm model, CancellationToken cancellationToken)
        {
         
            var currentUser = await _workContext.CurrentUserAsync();
            if (model == null)
                throw new Exception("Invalid vendor data.");

            model.Email = (model.Email ?? string.Empty).Trim();
            model.Phone = (model.Phone ?? string.Empty).Trim();
            model.Name = (model.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new Exception("Email is required.");

            var vendorExists = await _unitOfWork.Vendors.AnyAsync(
                x => x.Email == model.Email && x.IsDelete != 1, cancellationToken);

            if (vendorExists)
                throw new Exception("Vendor with this email already exists.");

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                throw new Exception("A user with this email already exists.");

           // await EnsureRoleExistsAsync(VendorRoleName);

            ApplicationUser? vendorUser = null;
            var temporaryPassword = GenerateTemporaryPassword();

            try
            {
                vendorUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PhoneNumber = model.Phone,
                    FullName = model.Name,
                    EntryBy = currentUser.FullName,
                    CreatedDate = DateTime.UtcNow
                };

                var userCreateResult = await _userManager.CreateAsync(vendorUser, model.Phone.Trim());
                if (!userCreateResult.Succeeded)
                {
                    var errors = string.Join(" ", userCreateResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create vendor user. {errors}".Trim());
                }

                var addRoleResult = await _userManager.AddToRoleAsync(vendorUser, VendorRoleName);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(" ", addRoleResult.Errors.Select(e => e.Description));
                    await _userManager.DeleteAsync(vendorUser);
                    vendorUser = null;
                    throw new Exception($"Failed to assign Vendor role. {errors}".Trim());
                }

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
                    CreatedBy = currentUser.FullName,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new VendorCreateResultVm
                {
                    VendorId = vendor.Id,
                    Email = model.Email,
                    TemporaryPassword = temporaryPassword
                };
            }
            catch
            {
                if (vendorUser != null)
                    await _userManager.DeleteAsync(vendorUser);

                throw;
            }
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
            var currentUser = await _workContext.CurrentUserAsync();
            if (model == null)
                throw new Exception("Invalid vendor data.");

            model.Email = (model.Email ?? string.Empty).Trim();
            model.Phone = (model.Phone ?? string.Empty).Trim();
            model.Name = (model.Name ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new Exception("Email is required.");

            var vendor = await _unitOfWork.Vendors.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            var vendorEmailBefore = (vendor.Email ?? string.Empty).Trim();

            var vendorDuplicateEmailExists = await _unitOfWork.Vendors.AnyAsync(
                x => x.Email == model.Email && x.Id != model.Id && x.IsDelete == 0);

            if (vendorDuplicateEmailExists)
                throw new Exception("Another vendor with the same email exists.");

            // Update Identity user linked by vendor email
            var vendorUser = await _userManager.FindByEmailAsync(vendorEmailBefore);
            if (vendorUser == null && !string.Equals(vendorEmailBefore, model.Email, StringComparison.OrdinalIgnoreCase))
                vendorUser = await _userManager.FindByEmailAsync(model.Email);

            if (vendorUser == null)
                throw new Exception("Vendor user not found for this vendor. Please create the vendor user first.");

            var anotherUserWithNewEmail = await _userManager.FindByEmailAsync(model.Email);
            if (anotherUserWithNewEmail != null && anotherUserWithNewEmail.Id != vendorUser.Id)
                throw new Exception("Another user with the same email exists.");

            vendorUser.FullName = model.Name;
            vendorUser.PhoneNumber = model.Phone;

            if (!string.Equals(vendorUser.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(vendorUser, model.Email);
                if (!setEmail.Succeeded)
                {
                    var errors = string.Join(" ", setEmail.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to update vendor user email. {errors}".Trim());
                }

                var setUserName = await _userManager.SetUserNameAsync(vendorUser, model.Email);
                if (!setUserName.Succeeded)
                {
                    var errors = string.Join(" ", setUserName.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to update vendor username. {errors}".Trim());
                }
            }

            var userUpdate = await _userManager.UpdateAsync(vendorUser);
            if (!userUpdate.Succeeded)
            {
                var errors = string.Join(" ", userUpdate.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update vendor user. {errors}".Trim());
            }

            // Update vendor entity
            vendor.Name = model.Name;
            vendor.Phone = model.Phone;
            vendor.Email = model.Email;
            vendor.Address = model.Address;
            vendor.CompanyName = model.CompanyName;
            vendor.CompanyWebsite = model.CompanyWebsite;
            vendor.Notes = model.Notes;
            vendor.IsActive = model.IsActive;
            vendor.UpdatedBy = currentUser.FullName;
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

        private async Task EnsureRoleExistsAsync(string role)
        {
            if (await _roleManager.RoleExistsAsync(role))
                return;

            var result = await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = role,
                IsSystem = false
            });

            if (result.Succeeded)
                return;

            // Race condition: another request created it after RoleExistsAsync
            if (await _roleManager.RoleExistsAsync(role))
                return;

            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create role '{role}'. {errors}".Trim());
        }

        private static string GenerateTemporaryPassword(int length = 12)
        {
            if (length < 8) length = 8;

            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$%";
            var all = uppercase + lowercase + digits + special;

            var chars = new char[length];
            chars[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            chars[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (var i = 4; i < length; i++)
                chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}

