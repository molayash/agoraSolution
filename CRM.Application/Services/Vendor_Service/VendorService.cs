using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Email_Service;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Constants;
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
        private readonly IEmailService _emailService;

        public VendorService(
            IWorkContext workContext,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IEmailService emailService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<VendorCreateResultVm> Add(VendorVm model, CancellationToken cancellationToken)
        {
            var currentUser = await _workContext.CurrentUserAsync() ?? throw new Exception("Unauthorized request.");

            NormalizeVendor(model, VendorStatuses.Active);
            var normalizedStatus = VendorStatuses.Normalize(model.Status, VendorStatuses.Active);

            ApplicationUser? vendorUser = null;
            string temporaryPassword = string.Empty;

            try
            {
                if (ShouldCreateLoginAccount(normalizedStatus))
                {
                    var account = await CreateVendorIdentityAccountAsync(
                        model.Name,
                        model.Email,
                        model.Phone,
                        currentUser.FullName ?? "Admin",
                        null,
                        null,
                        cancellationToken);

                    vendorUser = account.User;
                    temporaryPassword = account.TemporaryPassword;
                }

                var vendor = new Vendor();
                ApplyVendorChanges(vendor, model, normalizedStatus, currentUser.FullName);
                vendor.CreatedAt = DateTime.UtcNow;
                vendor.CreatedBy = currentUser.FullName;
                vendor.IsDelete = 0;
                vendor.UserId = vendorUser?.Id;

                await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new VendorCreateResultVm
                {
                    VendorId = vendor.Id,
                    Email = vendor.Email,
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

        public async Task<long> SubmitRegistrationRequest(VendorRegistrationRequestVm model, CancellationToken cancellationToken)
        {
            NormalizeRegistrationRequest(model);
            await EnsureVendorEmailAvailableAsync(model.Email, null, null, cancellationToken);

            ApplicationUser? vendorUser = null;
            Vendor? vendor = null;
            string temporaryPassword = string.Empty;

            try
            {
                var account = await CreateVendorIdentityAccountAsync(
                    model.Name,
                    model.Email,
                    model.Phone,
                    "Vendor Registration Portal",
                    null,
                    null,
                    cancellationToken);

                vendorUser = account.User;
                temporaryPassword = account.TemporaryPassword;

                vendor = new Vendor
                {
                    Name = model.Name,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    CompanyName = model.CompanyName,
                    CompanyWebsite = NormalizeWebsite(model.CompanyWebsite),
                    Notes = NormalizeOptionalText(model.Notes),
                    Status = VendorStatuses.Pending,
                    IsActive = false,
                    IsDelete = 0,
                    CreatedBy = "Vendor Registration Portal",
                    CreatedAt = DateTime.UtcNow,
                    UserId = vendorUser.Id
                };

                await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await SendVendorRegistrationEmailAsync(model.Name, model.Email, temporaryPassword);

                return vendor.Id;
            }
            catch
            {
                if (vendor != null && vendor.Id > 0)
                {
                    _unitOfWork.Vendors.Remove(vendor);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                if (vendorUser != null)
                    await _userManager.DeleteAsync(vendorUser);

                throw;
            }
        }

        public async Task<List<VendorVm>> GetAll(CancellationToken cancellationToken)
        {
            return await _unitOfWork.Vendors.Query()
                .Where(item => item.IsDelete == 0)
                .OrderBy(item =>
                    item.Status == VendorStatuses.Pending ? 0 :
                    item.Status == VendorStatuses.Partial ? 1 :
                    item.Status == VendorStatuses.Active ? 2 : 3)
                .ThenByDescending(item => item.Id)
                .Select(item => new VendorVm
                {
                    Id = item.Id,
                    Name = item.Name,
                    Phone = item.Phone,
                    Email = item.Email,
                    Address = item.Address,
                    CompanyName = item.CompanyName,
                    CompanyWebsite = item.CompanyWebsite,
                    Notes = item.Notes,
                    UserId = item.UserId,
                    Status = VendorStatuses.Normalize(item.Status, item.IsActive ? VendorStatuses.Active : VendorStatuses.Pending),
                    IsActive = item.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<VendorVm> GetById(long id)
        {
            var vendor = await _unitOfWork.Vendors.Query()
                .Where(item => item.Id == id && item.IsDelete == 0)
                .Select(item => new VendorVm
                {
                    Id = item.Id,
                    Name = item.Name,
                    Phone = item.Phone,
                    Email = item.Email,
                    Address = item.Address,
                    CompanyName = item.CompanyName,
                    CompanyWebsite = item.CompanyWebsite,
                    Notes = item.Notes,
                    UserId = item.UserId,
                    Status = VendorStatuses.Normalize(item.Status, item.IsActive ? VendorStatuses.Active : VendorStatuses.Pending),
                    IsActive = item.IsActive
                })
                .FirstOrDefaultAsync();

            if (vendor == null)
                throw new Exception("Vendor not found.");

            return vendor;
        }

        public async Task<VendorCreateResultVm> Update(VendorVm model, CancellationToken cancellationToken)
        {
            var currentUser = await _workContext.CurrentUserAsync() ?? throw new Exception("Unauthorized request.");

            NormalizeVendor(model, VendorStatuses.Pending);

            var vendor = await _unitOfWork.Vendors.Query()
                .FirstOrDefaultAsync(item => item.Id == model.Id && item.IsDelete == 0, cancellationToken);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            var normalizedStatus = VendorStatuses.Normalize(model.Status, VendorStatuses.Pending);
            var vendorUser = await ResolveVendorUserAsync(vendor);

            await EnsureVendorEmailAvailableAsync(model.Email, vendor.Id, vendorUser?.Id, cancellationToken);

            var createdNewAccount = false;
            string temporaryPassword = string.Empty;

            try
            {
                if (vendorUser == null && ShouldCreateLoginAccount(normalizedStatus))
                {
                    var account = await CreateVendorIdentityAccountAsync(
                        model.Name,
                        model.Email,
                        model.Phone,
                        currentUser.FullName ?? "Admin",
                        vendor.Id,
                        null,
                        cancellationToken);

                    vendorUser = account.User;
                    temporaryPassword = account.TemporaryPassword;
                    createdNewAccount = true;
                }
                else if (vendorUser != null)
                {
                    await SyncVendorIdentityAccountAsync(vendorUser, model.Name, model.Phone, model.Email);

                    if (ShouldCreateLoginAccount(normalizedStatus) && !await _userManager.HasPasswordAsync(vendorUser))
                    {
                        temporaryPassword = GenerateTemporaryPassword();
                        var addPasswordResult = await _userManager.AddPasswordAsync(vendorUser, vendor.Phone);
                        if (!addPasswordResult.Succeeded)
                            throw new Exception(string.Join(" ", addPasswordResult.Errors.Select(item => item.Description)));
                    }
                }

                ApplyVendorChanges(vendor, model, normalizedStatus, currentUser.FullName);
                vendor.UserId = vendorUser?.Id;
                vendor.UpdatedAt = DateTime.UtcNow;
                vendor.UpdatedBy = currentUser.FullName;

                _unitOfWork.Vendors.Update(vendor);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new VendorCreateResultVm
                {
                    VendorId = vendor.Id,
                    Email = vendor.Email,
                    TemporaryPassword = temporaryPassword
                };
            }
            catch
            {
                if (createdNewAccount && vendorUser != null)
                    await _userManager.DeleteAsync(vendorUser);

                throw;
            }
        }

        public async Task<bool> Delete(long id)
        {
            var user = await _workContext.CurrentUserAsync();

            var vendor = await _unitOfWork.Vendors.Query()
                .FirstOrDefaultAsync(item => item.Id == id && item.IsDelete == 0);

            if (vendor == null)
                throw new Exception("Vendor not found.");

            vendor.IsDelete = 1;
            vendor.UpdatedBy = user?.FullName;
            vendor.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static bool ShouldCreateLoginAccount(string status) =>
            string.Equals(status, VendorStatuses.Active, StringComparison.OrdinalIgnoreCase);

        private void NormalizeVendor(VendorVm model, string defaultStatus)
        {
            if (model == null)
                throw new Exception("Invalid vendor data.");

            model.Name = NormalizeRequiredText(model.Name, "Vendor name is required.");
            model.Phone = NormalizeRequiredText(model.Phone, "Phone number is required.");
            model.Email = NormalizeRequiredText(model.Email, "Email is required.");
            model.CompanyName = NormalizeRequiredText(model.CompanyName, "Company name is required.");
            model.Address = NormalizeOptionalText(model.Address);
            model.CompanyWebsite = NormalizeWebsite(model.CompanyWebsite);
            model.Notes = NormalizeOptionalText(model.Notes);
            model.Status = VendorStatuses.Normalize(model.Status, defaultStatus);
        }

        private void NormalizeRegistrationRequest(VendorRegistrationRequestVm model)
        {
            if (model == null)
                throw new Exception("Invalid vendor data.");

            model.Name = NormalizeRequiredText(model.Name, "Vendor name is required.");
            model.Phone = NormalizeRequiredText(model.Phone, "Phone number is required.");
            model.Email = NormalizeRequiredText(model.Email, "Email is required.");
            model.CompanyName = NormalizeRequiredText(model.CompanyName, "Company name is required.");
            model.Address = NormalizeOptionalText(model.Address);
            model.CompanyWebsite = NormalizeWebsite(model.CompanyWebsite);
            model.Notes = NormalizeOptionalText(model.Notes);
        }

        private static string NormalizeRequiredText(string? value, string errorMessage)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new Exception(errorMessage);

            return normalized;
        }

        private static string? NormalizeOptionalText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeWebsite(string? value)
        {
            var normalized = NormalizeOptionalText(value);
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            return normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : $"https://{normalized}";
        }

        private static void ApplyVendorChanges(Vendor vendor, VendorVm model, string normalizedStatus, string? actorName)
        {
            vendor.Name = model.Name;
            vendor.Phone = model.Phone;
            vendor.Email = model.Email;
            vendor.Address = model.Address;
            vendor.CompanyName = model.CompanyName;
            vendor.CompanyWebsite = model.CompanyWebsite;
            vendor.Notes = model.Notes;
            vendor.Status = normalizedStatus;
            vendor.IsActive = ShouldCreateLoginAccount(normalizedStatus);

            if (vendor.CreatedAt == default)
            {
                vendor.CreatedAt = DateTime.UtcNow;
                vendor.CreatedBy = actorName;
            }
        }

        private async Task<ApplicationUser?> ResolveVendorUserAsync(Vendor vendor)
        {
            if (!string.IsNullOrWhiteSpace(vendor.UserId))
            {
                var byId = await _userManager.FindByIdAsync(vendor.UserId);
                if (byId != null)
                    return byId;
            }

            if (string.IsNullOrWhiteSpace(vendor.Email))
                return null;

            return await _userManager.FindByEmailAsync(vendor.Email);
        }

        private async Task SyncVendorIdentityAccountAsync(ApplicationUser vendorUser, string name, string phone, string email)
        {
            vendorUser.FullName = name;
            vendorUser.PhoneNumber = phone;

            if (!string.Equals(vendorUser.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(vendorUser, email);
                if (!setEmail.Succeeded)
                    throw new Exception(string.Join(" ", setEmail.Errors.Select(item => item.Description)));

                var setUserName = await _userManager.SetUserNameAsync(vendorUser, email);
                if (!setUserName.Succeeded)
                    throw new Exception(string.Join(" ", setUserName.Errors.Select(item => item.Description)));
            }

            var updateResult = await _userManager.UpdateAsync(vendorUser);
            if (!updateResult.Succeeded)
                throw new Exception(string.Join(" ", updateResult.Errors.Select(item => item.Description)));
        }

        private async Task EnsureVendorEmailAvailableAsync(string email, long? vendorId, string? existingUserId, CancellationToken cancellationToken)
        {
            var duplicateVendorExists = await _unitOfWork.Vendors.AnyAsync(
                item => item.Email == email && item.IsDelete == 0 && (!vendorId.HasValue || item.Id != vendorId.Value),
                cancellationToken);

            if (duplicateVendorExists)
                throw new Exception("Vendor with this email already exists.");

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && existingUser.Id != existingUserId)
                throw new Exception("A user with this email already exists.");
        }

        private async Task<(ApplicationUser User, string TemporaryPassword)> CreateVendorIdentityAccountAsync(
            string name,
            string email,
            string phone,
            string entryBy,
            long? vendorId,
            string? existingUserId,
            CancellationToken cancellationToken,
            bool createPassword = true)
        {
            await EnsureVendorEmailAvailableAsync(email, vendorId, existingUserId, cancellationToken);
            await EnsureRoleExistsAsync(VendorRoleName);

            var temporaryPassword = phone;
            var vendorUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phone,
                FullName = name,
                EntryBy = entryBy,
                CreatedDate = DateTime.UtcNow
            };

            var createResult = createPassword
                ? await _userManager.CreateAsync(vendorUser, temporaryPassword)
                : await _userManager.CreateAsync(vendorUser);

            if (!createResult.Succeeded)
                throw new Exception(string.Join(" ", createResult.Errors.Select(item => item.Description)));

            var roleResult = await _userManager.AddToRoleAsync(vendorUser, VendorRoleName);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(vendorUser);
                throw new Exception(string.Join(" ", roleResult.Errors.Select(item => item.Description)));
            }

            return (vendorUser, temporaryPassword);
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

            if (!result.Succeeded && !await _roleManager.RoleExistsAsync(role))
                throw new Exception(string.Join(" ", result.Errors.Select(item => item.Description)));
        }

        private async Task SendVendorRegistrationEmailAsync(string vendorName, string email, string temporaryPassword)
        {
            var subject = "Agora Food vendor registration received";
            var message = BuildVendorRegistrationEmailBody(vendorName, email, temporaryPassword);
            var emailSent = await _emailService.SendEmailAsync(
                email,
                subject,
                message,
                badgeText: "Awaiting Approval",
                titleText: "Vendor Registration Received");

            if (!emailSent)
                throw new Exception("We could not send your vendor credential email, so the registration request was not completed. Please try again.");
        }

        private static string BuildVendorRegistrationEmailBody(string vendorName, string email, string temporaryPassword)
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Dear {vendorName},");
            builder.AppendLine();
            builder.AppendLine("Thank you for registering as a vendor with Agora Food.");
            builder.AppendLine("We have received your registration request and it is now awaiting admin approval.");
            builder.AppendLine();
            builder.AppendLine("Your login details:");
            builder.AppendLine($"Email: {email}");
            builder.AppendLine($"Password: {temporaryPassword}");
            builder.AppendLine();
            builder.AppendLine("You can keep these credentials ready, but you will only be able to sign in after your account has been approved.");
            builder.AppendLine("We will notify you once your vendor account is activated.");
            builder.AppendLine();
            builder.AppendLine("Best regards,");
            builder.AppendLine("Agora Food Team");

            return builder.ToString();
        }

        private static string GenerateTemporaryPassword(int length = 12)
        {
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
