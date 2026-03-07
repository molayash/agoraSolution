using CRM.Application.Interfaces.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.ContactMessage_Service
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactMessageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ContactMessageVm>> GetListAsync()
        {
            return await _unitOfWork.ContactMessages.Query()
                .Where(x => x.IsDelete != 1)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ContactMessageVm
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Message = x.Message,
                    IsSeen = x.IsSeen,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> AddAsync(ContactMessageVm model)
        {
            var entity = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Message = model.Message,
                IsSeen = false,
                CreatedAt = DateTime.Now,
                IsDelete = 0
            };

            await _unitOfWork.ContactMessages.AddAsync(entity);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsSeenAsync(long id)
        {
            var entity = await _unitOfWork.ContactMessages.Query()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            entity.IsSeen = true;
            entity.UpdatedAt = DateTime.Now;

            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _unitOfWork.ContactMessages.Query()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.Now;

            return await _unitOfWork.SaveChangesAsync() > 0;
        }
    }
}
