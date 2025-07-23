using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho AboutMeService
    /// </summary>
    public interface IAboutMeService
    {
        Task<AboutMe?> GetAboutMeAsync();
        Task<AboutMe> CreateOrUpdateAboutMeAsync(AboutMe aboutMe);
        Task<bool> DeleteAboutMeAsync();
    }

    /// <summary>
    /// Service để quản lý About Me
    /// </summary>
    public class AboutMeService : IAboutMeService
    {
        private readonly ApplicationDbContext _context;

        public AboutMeService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin About Me (chỉ có 1 record)
        /// </summary>
        public async Task<AboutMe?> GetAboutMeAsync()
        {
            return await _context.AboutMes.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tạo hoặc cập nhật About Me
        /// </summary>
        public async Task<AboutMe> CreateOrUpdateAboutMeAsync(AboutMe aboutMe)
        {
            var existing = await _context.AboutMes.FirstOrDefaultAsync();
            
            if (existing == null)
            {
                // Tạo mới
                aboutMe.UpdatedAt = DateTime.UtcNow;
                _context.AboutMes.Add(aboutMe);
            }
            else
            {
                // Cập nhật
                existing.FullName = aboutMe.FullName;
                existing.JobTitle = aboutMe.JobTitle;
                existing.AvatarURL = aboutMe.AvatarURL;
                existing.BioSummary = aboutMe.BioSummary;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return existing ?? aboutMe;
        }

        /// <summary>
        /// Xóa About Me
        /// </summary>
        public async Task<bool> DeleteAboutMeAsync()
        {
            var existing = await _context.AboutMes.FirstOrDefaultAsync();
            if (existing != null)
            {
                _context.AboutMes.Remove(existing);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
