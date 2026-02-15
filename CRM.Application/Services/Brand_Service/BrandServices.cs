using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Brand_Service
{
    public class BrandServices : IBrandService
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;

        public BrandServices(IWorkContext workContext, CrmDbContext context)
        {
            _workContext = workContext;
            _context = context;
        }

        // ✅ ADD BRAND
        public async Task<bool> Add(BrandVm model, CancellationToken cancellationToken)
        {
            var user = await _workContext.CurrentUserAsync();
            if (model == null) return false;

            // Prevent duplicate brand
            var exists = await _context.Brands
                .AnyAsync(x => x.Name == model.Name && x.IsDelete == 0, cancellationToken);

            if (exists)
                throw new Exception("Brand already exists.");

            var brand = new Brand
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                IsDelete = 0,
                CreatedBy = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Brands.AddAsync(brand, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        // ✅ GET ALL
        public async Task<List<BrandVm>> GetAll(CancellationToken cancellationToken)
        {
            return await _context.Brands
                .Where(x => x.IsDelete == 0)
                .OrderByDescending(x => x.Id)
                .Select(x => new BrandVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl
                }).OrderBy(c=>c.Id)
                .ToListAsync(cancellationToken);
        }

        // ✅ GET BY ID
        public async Task<BrandVm> GetById(long Id)
        {
            var brand = await _context.Brands
                .Where(x => x.Id == Id && x.IsDelete == 0)
                .Select(x => new BrandVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (brand == null)
                throw new Exception("Brand not found.");

            return brand;
        }

        // ✅ UPDATE
        public async Task<bool> Update(BrandVm model)
        {
            var user = await _workContext.CurrentUserAsync();
            var brand = await _context.Brands
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (brand == null)
                throw new Exception("Brand not found.");

            // Check duplicate
            var exists = await _context.Brands
                .AnyAsync(x => x.Name == model.Name && x.Id != model.Id && x.IsDelete == 0);

            if (exists)
                throw new Exception("Another brand with same name exists.");

            brand.Name = model.Name;
            brand.Description = model.Description;
            brand.ImageUrl = model.ImageUrl;
            brand.UpdatedBy = user.FullName;
            brand.UpdatedAt = DateTime.UtcNow;

            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();

            return true;
        }

        // ✅ SOFT DELETE
        public async Task<bool> Delete(long Id)
        {
            var user = await _workContext.CurrentUserAsync();
            var brand = await _context.Brands
                .FirstOrDefaultAsync(x => x.Id == Id && x.IsDelete == 0);

            if (brand == null)
                throw new Exception("Brand not found.");

            brand.IsDelete = 1;
            brand.UpdatedBy = user.FullName;
            brand.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
