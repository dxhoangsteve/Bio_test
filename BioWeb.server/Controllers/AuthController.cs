using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;
using BioWeb.Server.Models;
using BioWeb.Server.Services;
using BioWeb.Server.Attributes;
using BioWeb.Shared.Models.DTOs;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext context, IAuthService authService, ITokenService tokenService)
        {
            _context = context;
            _authService = authService;
            _tokenService = tokenService;
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

        /// <summary>
        /// Login admin và trả về JWT token
        /// </summary>
        [HttpPost("admin/login")]
        public async Task<ActionResult<LoginResponse>> AdminLogin([FromBody] AdminLoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Username và password là bắt buộc"
                    });
                }

                // Validate admin credentials
                var isValid = await _authService.IsValidAdminAsync(request.Username, request.Password);

                if (!isValid)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Username hoặc password không đúng"
                    });
                }

                // Generate JWT token
                var token = _tokenService.GenerateToken(request.Username);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        [HttpGet("validate-token")]
        [AdminAuth]
        public IActionResult ValidateToken()
        {
            // Nếu đến được đây nghĩa là token hợp lệ (đã qua AdminAuth)
            return Ok(new { Success = true, Message = "Token hợp lệ" });
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

    /// <summary>
    /// Request model cho admin login
    /// </summary>
    public class AdminLoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
