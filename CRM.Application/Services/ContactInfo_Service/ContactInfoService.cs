using CRM.Application.Interfaces.Repositories;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.ContactInfo_Service
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactInfoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ContactInfoVm> GetContactInfoAsync()
        {
            var entity = await _unitOfWork.ContactInfos.Query().FirstOrDefaultAsync();
            if (entity == null)
                return new ContactInfoVm();

            return new ContactInfoVm
            {
                Id = entity.Id,
                Phone1 = entity.Phone1,
                Phone2 = entity.Phone2,
                Email1 = entity.Email1,
                Email2 = entity.Email2,
                Website1 = entity.Website1,
                Website2 = entity.Website2,
                HeadOffice = entity.HeadOffice,
                BangladeshOffice = entity.BangladeshOffice
            };
        }

        public async Task<bool> UpdateContactInfoAsync(ContactInfoVm model)
        {
            var entity = await _unitOfWork.ContactInfos.Query().FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new ContactInfo();
                _unitOfWork.ContactInfos.Add(entity);
            }

            entity.Phone1 = model.Phone1;
            entity.Phone2 = model.Phone2;
            entity.Email1 = model.Email1;
            entity.Email2 = model.Email2;
            entity.Website1 = model.Website1;
            entity.Website2 = model.Website2;
            entity.HeadOffice = model.HeadOffice;
            entity.BangladeshOffice = model.BangladeshOffice;
            entity.UpdatedAt = DateTime.Now;

            return await _unitOfWork.SaveChangesAsync() > 0;
        }
    }
}
