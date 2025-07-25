using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho SiteConfigurationService
    /// </summary>
    public interface ISiteConfigurationService
    {
        Task<SiteConfiguration?> GetSiteConfigurationAsync();
        Task<bool> UpdateSiteConfigurationAsync(SiteConfiguration config);
        Task<bool> ResetSiteConfigurationAsync(int id);
        Task<SiteConfiguration> GetOrCreateDefaultConfigAsync();
        Task<bool> IncrementViewCountAsync();
        Task<bool> IncrementViewCountAsync(string clientIp);
    }

    /// <summary>
    /// Service để quản lý các thao tác liên quan đến SiteConfiguration.
    /// </summary>
    public class SiteConfigurationService : ISiteConfigurationService
    {
        private readonly ApplicationDbContext _context;
        private static readonly Dictionary<string, DateTime> _lastViewTimes = new();
        private static readonly TimeSpan _viewCooldown = TimeSpan.FromMinutes(5); // 5 phút cooldown

        /// <summary>
        /// Khởi tạo một phiên bản mới của SiteConfigurationService.
        /// </summary>
        public SiteConfigurationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// </summary>
        /// <returns>Đối tượng SiteConfiguration nếu tìm thấy, ngược lại là null.</returns>
        public async Task<SiteConfiguration?> GetSiteConfigurationAsync()
        {
            return await _context.SiteConfigurations.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Tạo một cấu hình site mới (private method).
        /// </summary>
        /// <param name="config">Đối tượng SiteConfiguration cần tạo.</param>
        /// <returns>Đối tượng SiteConfiguration đã được tạo.</returns>
        private async Task<SiteConfiguration> CreateSiteConfigurationAsync(SiteConfiguration config)
        {
            config.UpdatedAt = DateTime.UtcNow;
            _context.SiteConfigurations.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        /// <summary>
        /// Cập nhật cấu hình site hiện có.
        /// </summary>
        /// <param name="config">Đối tượng SiteConfiguration cần cập nhật.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại là false.</returns>
        public async Task<bool> UpdateSiteConfigurationAsync(SiteConfiguration config)
        {
            config.UpdatedAt = DateTime.UtcNow;
            _context.Entry(config).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SiteConfigurationExists(config.ConfigID))
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
        /// Reset cấu hình site về giá trị mặc định.

        public async Task<bool> ResetSiteConfigurationAsync(int id)
        {
            var config = await _context.SiteConfigurations.FindAsync(id);
            if (config == null)
            {
                return false;
            }

            // Reset về giá trị mặc định
            config.FullName = "Chưa cập nhật";
            config.JobTitle = "";
            config.AvatarURL = "";
            config.BioSummary = "";
            config.Email = "";
            config.PhoneNumber = "";
            config.Address = "";
            config.CV_FilePath = "";
            config.FacebookURL = "";
            config.GitHubURL = "";
            config.LinkedInURL = "";
            config.ViewCount = 0;
            config.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy hoặc tạo cấu hình mặc định nếu chưa có.
        public async Task<SiteConfiguration> GetOrCreateDefaultConfigAsync()
        {
            var config = await GetSiteConfigurationAsync();

            if (config == null)
            {
                config = new SiteConfiguration
                {
                    FullName = "Chưa cập nhật",
                    JobTitle = "",
                    AvatarURL = "",
                    BioSummary = "",
                    Email = "",
                    PhoneNumber = "",
                    Address = "",
                    CV_FilePath = "",
                    FacebookURL = "",
                    GitHubURL = "",
                    LinkedInURL = "",
                    ViewCount = 0,
                    UpdatedAt = DateTime.UtcNow
                };

                config = await CreateSiteConfigurationAsync(config);
            }

            return config;
        }

        /// <summary>
        /// Tăng view count cho site configuration.
        /// </summary>
        /// <returns>True nếu tăng thành công, ngược lại là false.</returns>
        public async Task<bool> IncrementViewCountAsync()
        {
            try
            {
                var config = await GetOrCreateDefaultConfigAsync();
                config.ViewCount++;
                config.UpdatedAt = DateTime.UtcNow;

                _context.Entry(config).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tăng view count với IP tracking để chống spam.
        /// </summary>
        /// <param name="clientIp">IP của client</param>
        /// <returns>True nếu tăng thành công, ngược lại là false.</returns>
        public async Task<bool> IncrementViewCountAsync(string clientIp)
        {
            try
            {
                // Kiểm tra cooldown
                if (_lastViewTimes.ContainsKey(clientIp))
                {
                    var lastView = _lastViewTimes[clientIp];
                    if (DateTime.UtcNow - lastView < _viewCooldown)
                    {
                        // Còn trong thời gian cooldown, không tăng view
                        return false;
                    }
                }

                // Cập nhật thời gian view cuối
                _lastViewTimes[clientIp] = DateTime.UtcNow;

                // Dọn dẹp các IP cũ (giữ lại 1000 IP gần nhất)
                if (_lastViewTimes.Count > 1000)
                {
                    var oldEntries = _lastViewTimes
                        .OrderBy(x => x.Value)
                        .Take(_lastViewTimes.Count - 1000)
                        .ToList();

                    foreach (var entry in oldEntries)
                    {
                        _lastViewTimes.Remove(entry.Key);
                    }
                }

                // Tăng view count
                var config = await GetOrCreateDefaultConfigAsync();
                config.ViewCount++;
                config.UpdatedAt = DateTime.UtcNow;

                _context.Entry(config).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem một cấu hình site có tồn tại không.
        /// </summary>
        /// <param name="id">ID của cấu hình.</param>
        /// <returns>True nếu tồn tại, ngược lại là false.</returns>
        private async Task<bool> SiteConfigurationExists(int id)
        {
            return await _context.SiteConfigurations.AnyAsync(e => e.ConfigID == id);
        }
    }
}
