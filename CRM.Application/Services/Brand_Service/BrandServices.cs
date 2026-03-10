using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Brand_Service
{
    public class BrandServices : IBrandService
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;

        public BrandServices(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Add(BrandVm model, CancellationToken cancellationToken)
        {
            var user = await _workContext.CurrentUserAsync();
            if (model == null) return false;

            var exists = await _unitOfWork.Brands.AnyAsync(
                x => x.Name == model.Name && x.IsDelete == 0, cancellationToken);

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

            await _unitOfWork.Brands.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<List<BrandVm>> GetAll(CancellationToken cancellationToken)
        {
            return await _unitOfWork.Brands.Query()
                .Where(x => x.IsDelete == 0)
                .OrderBy(x => x.Id)
                .Select(x => new BrandVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<BrandVm> GetById(long Id)
        {
            var brand = await _unitOfWork.Brands.Query()
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

        public async Task<bool> Update(BrandVm model)
        {
            var user = await _workContext.CurrentUserAsync();
            var brand = await _unitOfWork.Brands.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0);

            if (brand == null)
                throw new Exception("Brand not found.");

            var exists = await _unitOfWork.Brands.AnyAsync(
                x => x.Name == model.Name && x.Id != model.Id && x.IsDelete == 0);

            if (exists)
                throw new Exception("Another brand with same name exists.");

            brand.Name = model.Name;
            brand.Description = model.Description;
            brand.ImageUrl = model.ImageUrl;
            brand.UpdatedBy = user.FullName;
            brand.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Brands.Update(brand);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(long Id)
        {
            var user = await _workContext.CurrentUserAsync();
            var brand = await _unitOfWork.Brands.Query()
                .FirstOrDefaultAsync(x => x.Id == Id && x.IsDelete == 0);

            if (brand == null)
                throw new Exception("Brand not found.");

            brand.IsDelete = 1;
            brand.UpdatedBy = user.FullName;
            brand.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
