using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho ContactService
    /// </summary>
    public interface IContactService
    {
        Task<Contact?> GetContactAsync();
        Task<Contact> CreateOrUpdateContactAsync(Contact contact);
        Task<bool> DeleteContactAsync();
    }

    /// <summary>
    /// Service để quản lý Contact
    /// </summary>
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin Contact (chỉ có 1 record)
        /// </summary>
        public async Task<Contact?> GetContactAsync()
        {
            return await _context.Contacts.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tạo hoặc cập nhật Contact
        /// </summary>
        public async Task<Contact> CreateOrUpdateContactAsync(Contact contact)
        {
            var existing = await _context.Contacts.FirstOrDefaultAsync();
            
            if (existing == null)
            {
                // Tạo mới
                contact.UpdatedAt = DateTime.UtcNow;
                _context.Contacts.Add(contact);
            }
            else
            {
                // Cập nhật
                existing.Email = contact.Email;
                existing.PhoneNumber = contact.PhoneNumber;
                existing.Address = contact.Address;
                existing.GitHubURL = contact.GitHubURL;
                existing.LinkedInURL = contact.LinkedInURL;
                existing.FacebookURL = contact.FacebookURL;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existing ?? contact;
        }

        /// <summary>
        /// Xóa Contact
        /// </summary>
        public async Task<bool> DeleteContactAsync()
        {
            var existing = await _context.Contacts.FirstOrDefaultAsync();
            if (existing != null)
            {
                _context.Contacts.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
