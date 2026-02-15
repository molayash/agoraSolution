namespace CRM.Application.Services.Brand_Service
{
    public interface IBrandService
    {
        Task<bool> Add(BrandVm model, CancellationToken cancellationToken);
        Task<List<BrandVm>> GetAll(CancellationToken cancellationToken);
        Task<bool> Update(BrandVm model);
        Task<bool> Delete(long Id);
        Task<BrandVm> GetById(long Id);
    }
}
