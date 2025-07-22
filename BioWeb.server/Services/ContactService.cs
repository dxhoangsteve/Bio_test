using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho ContactService
    /// </summary>
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetAllContactsAsync();
        Task<Contact?> GetContactByIdAsync(int id);
        Task<bool> UpdateContactAsync(Contact contact);
        Task<bool> DeleteContactAsync(int id);
        Task<string?> GetContactEmailAsync();
        Task<bool> IncrementReadCountAsync();
    }

    /// <summary>
    /// Service để quản lý các thao tác liên quan đến Contact.
    /// </summary>
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Khởi tạo một phiên bản mới của ContactService.
        /// </summary>
        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả các liên hệ.
        /// </summary>
        public async Task<IEnumerable<Contact>> GetAllContactsAsync()
        {
            return await _context.Contacts.ToListAsync();
        }

        /// <summary>
        /// Lấy một liên hệ theo ID.
        /// </summary>
        public async Task<Contact?> GetContactByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        /// <summary>
        /// Lấy email contact cho guest (chỉ trả về email để hiển thị).
        /// </summary>
        public async Task<string?> GetContactEmailAsync()
        {
            var config = await _context.Contacts.FirstOrDefaultAsync();
            return config?.Email;
        }

        /// <summary>
        /// Cập nhật một liên hệ hiện có.
        /// </summary>
        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            _context.Entry(contact).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContactExists(contact.ContactID))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Xóa một liên hệ theo ID.
        /// </summary>
        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return false;
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tăng số lượt đọc contact (cho guest).
        /// </summary>
        public async Task<bool> IncrementReadCountAsync()
        {
            try
            {
                // Lấy contact đầu tiên hoặc tạo contact mặc định
                var contact = await _context.Contacts.FirstOrDefaultAsync();

                if (contact == null)
                {
                    // Tạo contact mặc định để track read count
                    contact = new Contact
                    {
                        FullName = "System Read Counter",
                        Email = "system@readcounter.com",
                        Message = "This contact is used for tracking read count",
                        SentDate = DateTime.UtcNow,
                        IsRead = true,
                        ReadCount = 1
                    };

                    _context.Contacts.Add(contact);
                }
                else
                {
                    contact.ReadCount++;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem liên hệ có tồn tại không.
        /// </summary>
        private async Task<bool> ContactExists(int id)
        {
            return await _context.Contacts.AnyAsync(e => e.ContactID == id);
        }
    }
}