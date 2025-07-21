using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;
using BioWeb.Server.Models;
using BioWeb.Server.Services;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm user theo username
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Username không tồn tại" });
            }

            // Verify password
            if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Password không đúng" });
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng nhập thành công",
                user = new
                {
                    user.UserID,
                    user.Username,
                    user.LastLogin
                }
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return NotFound(new { message = "User không tồn tại" });
            }

            // Verify old password
            if (!PasswordService.VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Password cũ không đúng" });
            }

            // Update password
            user.PasswordHash = PasswordService.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi password thành công" });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ChangePasswordRequest
    {
        public string Username { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
