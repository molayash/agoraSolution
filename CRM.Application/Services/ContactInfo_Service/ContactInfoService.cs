using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactInfo_Service
{
    public class ContactInfoService : IContactInfoService
    {
        private readonly CrmDbContext _context;

        public ContactInfoService(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<ContactInfoVm> GetContactInfoAsync()
        {
            var entity = await _context.ContactInfo.FirstOrDefaultAsync();
            if (entity == null)
            {
                return new ContactInfoVm();
            }

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
            var entity = await _context.ContactInfo.FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = new ContactInfo();
                _context.ContactInfo.Add(entity);
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

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
