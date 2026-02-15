using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactMessage_Service
{
    public class ContactMessageService : IContactMessageService
    {
        private readonly CrmDbContext _context;

        public ContactMessageService(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContactMessageVm>> GetListAsync()
        {
            return await _context.ContactMessages
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
                }).ToListAsync();
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

            await _context.ContactMessages.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAsSeenAsync(long id)
        {
            var entity = await _context.ContactMessages.FindAsync(id);
            if (entity == null) return false;

            entity.IsSeen = true;
            entity.UpdatedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _context.ContactMessages.FindAsync(id);
            if (entity == null) return false;

            entity.IsDelete = 1;
            entity.UpdatedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
