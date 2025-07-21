using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;
using BioWeb.Server.Models;

namespace BioWeb.Server.Services
{
    public interface IAuthService
    {
        Task<bool> IsValidAdminAsync(string username, string password);
        Task<AdminUser?> GetAdminByUsernameAsync(string username);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsValidAdminAsync(string username, string password)
        {
            var admin = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username);

            if (admin == null) return false;

            return PasswordService.VerifyPassword(password, admin.PasswordHash);
        }

        public async Task<AdminUser?> GetAdminByUsernameAsync(string username)
        {
            return await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
