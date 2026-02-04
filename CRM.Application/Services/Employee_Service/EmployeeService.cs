using CRM.Application.Interfaces.Medias;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using CRM.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Employee_Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly CrmDbContext _context;
        private readonly IMediaService mediaService;
        private readonly IWorkContext _workContext;
        public EmployeeService(
            CrmDbContext context,
            IMediaService mediaService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
             IWorkContext workContext)
        {
            _context = context;
            this.mediaService = mediaService;
            _userManager = userManager;
            _roleManager = roleManager;
            _workContext = workContext;
        }




        // ✅ ADD
        public async Task<EmployeeViewModel> AddAsync(EmployeeViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ Ensure role exists
                await EnsureEmployeeRoleExists();

                // CREATE APPLICATION USER

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.WorkEmail,
                    Email = model.WorkEmail,
                    FullName = model.FullName,
                    EntryBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var password = model.EmployeeCode;

                var createUser = await _userManager.CreateAsync(user, password);

                if (!createUser.Succeeded)
                {
                    var errors = string.Join(",", createUser.Errors.Select(e => e.Description));
                    throw new Exception(errors);
                }

                // ASSIGN ROLE

                await _userManager.AddToRoleAsync(user, "Employee");

                // CREATE EMPLOYEE

                var entity = new Employee
                {
                    EmployeeNumber = model.EmployeeNumber,
                    EmployeeCode = model.EmployeeCode,
                    FullName = model.FullName,
                    WorkEmail = model.WorkEmail,
                    PersonalEmail = model.PersonalEmail,
                    Mobile = model.Mobile,
                    EmploymentStatus = model.EmploymentStatus,
                    Gender = model.Gender,
                    JoiningDate = model.JoiningDate,
                    DateOfBirth = model.DateOfBirth,
                    DesignationID = model.DesignationID,
                    DepartmentID = model.DepartmentID,
                    ManagerID = model.ManagerID,
                    UserID = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsDelete = 0
                };

                if (model.ProfileImage is { Length: > 0 })
                {
                    entity.PhotoUrlWithPath =
                        mediaService.UploadFile(model.ProfileImage, "empImage");
                }

                _context.Employees.Add(entity);
                await _context.SaveChangesAsync();
                var role = await _roleManager.FindByNameAsync("Employee");
                var defaultPermissions = await _context.DefultMenuRolePermissions.Where(f=>f.RoleId== role.Id).ToListAsync();
                if (defaultPermissions.Any())
                {
                    var menuPermissions = defaultPermissions.Select(p => new MenuPermission
                    {
                        UserId = user.Id,
                        MenuId = p.MenuId,
                        CanView = p.CanView,
                        CanAdd = p.CanAdd,
                        CanEdit = p.CanEdit,
                        CanDelete = p.CanDelete
                    }).ToList();

                    await _context.MenuPermissions.AddRangeAsync(menuPermissions);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                model.Id = entity.Id;

                return model;
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw; // 🔥 NEVER RETURN SILENT ERROR
            }
        }






        // ✅ UPDATE
        public async Task<EmployeeViewModel?> UpdateAsync(EmployeeViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var entity = await _context.Employees
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null)
                    return null;

                //-----------------------------
                // UPDATE APPLICATION USER
                //-----------------------------
                if (!string.IsNullOrEmpty(entity.UserID))
                {
                    var user = await _userManager.FindByIdAsync(entity.UserID);

                    if (user != null)
                    {
                        user.FullName = model.FullName;
                        user.Email = model.WorkEmail;
                        user.UserName = model.WorkEmail;
                        user.PhoneNumber = model.Mobile;

                        var result = await _userManager.UpdateAsync(user);

                        if (!result.Succeeded)
                        {
                            var errors = string.Join(",", result.Errors.Select(e => e.Description));
                            throw new Exception(errors);
                        }
                    }
                }

                //-----------------------------
                // UPDATE EMPLOYEE
                //-----------------------------
                entity.EmployeeNumber = model.EmployeeNumber;
                entity.FullName = model.FullName;
                entity.WorkEmail = model.WorkEmail;
                entity.PersonalEmail = model.PersonalEmail;
                entity.Mobile = model.Mobile;
                entity.EmploymentStatus = model.EmploymentStatus;
                entity.Gender = model.Gender;
                entity.JoiningDate = model.JoiningDate;
                entity.DateOfBirth = model.DateOfBirth;
                entity.DesignationID = model.DesignationID;
                entity.DepartmentID = model.DepartmentID;
                entity.ManagerID = model.ManagerID;
                entity.UpdatedAt = DateTime.UtcNow;

                //-----------------------------
                // IMAGE UPDATE
                //-----------------------------
                if (model.ProfileImage is { Length: > 0 })
                {
                    entity.PhotoUrlWithPath =
                        mediaService.UpdateFile(entity.PhotoUrlWithPath, model.ProfileImage, "empImage");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // ✅ SOFT DELETE
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.Employees.FindAsync(id);

            if (entity == null)
                return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ GET BY ID
        public async Task<EmployeeViewModel?> GetByIdAsync(long id)
        {
            return await _context.Employees
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new EmployeeViewModel
                {
                    Id = x.Id,
                    EmployeeNumber = x.EmployeeNumber,
                    FullName = x.FullName,
                    WorkEmail = x.WorkEmail,
                    PersonalEmail = x.PersonalEmail,
                    Mobile = x.Mobile,
                    EmploymentStatus = x.EmploymentStatus,
                    Gender = x.Gender,
                    JoiningDate = x.JoiningDate,
                    DateOfBirth = x.DateOfBirth,
                    DesignationID = x.DesignationID,
                    DepartmentID = x.DepartmentID,
                    ManagerID = x.ManagerID,
                    UserID = x.UserID,
                    PhotoUrlWithPath = x.PhotoUrlWithPath,
                })
                .FirstOrDefaultAsync();
        }

        // ✅ GET ALL
        public async Task<IEnumerable<EmployeeViewModel>> GetAllAsync()
        {
            var query = from e in _context.Employees
                        join d in _context.Designations
                            on e.DesignationID equals d.Id
                        join dp in _context.Departments
                            on e.DepartmentID equals dp.Id
                        where e.IsDelete == 0
                        select new EmployeeViewModel
                        {
                            Id = e.Id,
                            EmployeeNumber = e.EmployeeNumber,
                            EmployeeCode = e.EmployeeCode,
                            FullName = e.FullName,
                            WorkEmail = e.WorkEmail,
                            PersonalEmail = e.PersonalEmail,
                            Mobile = e.Mobile,
                            EmploymentStatus = e.EmploymentStatus,
                            Gender = e.Gender,
                            JoiningDate = e.JoiningDate,
                            DateOfBirth = e.DateOfBirth,

                            DesignationID = e.DesignationID,
                            DepartmentID = e.DepartmentID,

                            // ✅ Joined values
                            DesignationName = d.Name,
                            DepartmentName = dp.Name,

                            ManagerID = e.ManagerID,
                            UserID = e.UserID,
                            PhotoUrlWithPath = e.PhotoUrlWithPath,
                        };

            return await query.ToListAsync();
        }



        private async Task EnsureEmployeeRoleExists()
        {
            const string roleName = "Employee";

            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var role = new ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName,
                    IsSystem =false
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create role '{roleName}': {errors}");
                }
            }
        }


    }

}
