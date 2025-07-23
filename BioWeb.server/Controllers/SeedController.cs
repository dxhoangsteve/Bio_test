using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _environment;

        public SeedController(IServiceProvider serviceProvider, IWebHostEnvironment environment)
        {
            _serviceProvider = serviceProvider;
            _environment = environment;
        }

        /// <summary>
        /// ⚠️ NGUY HIỂM: Xóa toàn bộ database và seed lại data mới
        /// CHỈ HOẠT ĐỘNG TRONG DEVELOPMENT ENVIRONMENT
        /// </summary>
        [HttpPost("force-seed")]
        public async Task<IActionResult> ForceSeed()
        {
            // 🔒 BẢO VỆ: Chỉ cho phép trong Development environment
            if (!_environment.IsDevelopment())
            {
                return Forbid("Endpoint này chỉ khả dụng trong Development environment để bảo vệ dữ liệu production!");
            }

            try
            {
                // Xóa data cũ
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Xóa theo thứ tự để tránh lỗi foreign key
                context.Articles.RemoveRange(context.Articles);
                context.Projects.RemoveRange(context.Projects);
                context.Categories.RemoveRange(context.Categories);
                context.SiteConfigurations.RemoveRange(context.SiteConfigurations);
                context.AdminUsers.RemoveRange(context.AdminUsers);

                await context.SaveChangesAsync();
                Console.WriteLine("🗑️ Đã xóa data cũ");

                // Seed data mới
                await SeedData.InitializeAsync(scope.ServiceProvider);

                return Ok(new {
                    message = "Seed data xong!",
                    warning = "⚠️ Toàn bộ dữ liệu cũ đã bị xóa và thay thế bằng dữ liệu mẫu",
                    environment = _environment.EnvironmentName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {
                    message = "Lỗi seed data",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra số lượng dữ liệu trong database
        /// CHỈ HOẠT ĐỘNG TRONG DEVELOPMENT ENVIRONMENT
        /// </summary>
        [HttpGet("check-data")]
        public async Task<IActionResult> CheckData()
        {
            // 🔒 BẢO VỆ: Chỉ cho phép trong Development environment
            if (!_environment.IsDevelopment())
            {
                return Forbid("Endpoint này chỉ khả dụng trong Development environment để bảo vệ thông tin hệ thống!");
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var result = new
            {
                AdminUsers = await context.AdminUsers.CountAsync(),
                SiteConfigurations = await context.SiteConfigurations.CountAsync(),
                Categories = await context.Categories.CountAsync(),
                Projects = await context.Projects.CountAsync(),
                Articles = await context.Articles.CountAsync(),
                Environment = _environment.EnvironmentName,
                Timestamp = DateTime.UtcNow
            };

            return Ok(result);
        }
    }
}
