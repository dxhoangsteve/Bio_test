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

        public SeedController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost("force-seed")]
        public async Task<IActionResult> ForceSeed()
        {
            try
            {
                // Xóa data cũ
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Xóa theo thứ tự để tránh lỗi foreign key
                context.Articles.RemoveRange(context.Articles);
                context.Contacts.RemoveRange(context.Contacts);
                context.Projects.RemoveRange(context.Projects);
                context.Categories.RemoveRange(context.Categories);
                context.SiteConfigurations.RemoveRange(context.SiteConfigurations);
                context.AdminUsers.RemoveRange(context.AdminUsers);

                await context.SaveChangesAsync();
                Console.WriteLine("🗑️ Đã xóa data cũ");

                // Seed data mới
                await SeedData.InitializeAsync(scope.ServiceProvider);

                return Ok(new { message = "Seed data xong!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new {
                    message = "Lỗi seed data",
                    error = ex.Message
                });
            }
        }

        [HttpGet("check-data")]
        public async Task<IActionResult> CheckData()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var result = new
            {
                AdminUsers = await context.AdminUsers.CountAsync(),
                SiteConfigurations = await context.SiteConfigurations.CountAsync(),
                Categories = await context.Categories.CountAsync(),
                Projects = await context.Projects.CountAsync(),
                Articles = await context.Articles.CountAsync(),
                Contacts = await context.Contacts.CountAsync()
            };
            
            return Ok(result);
        }
    }
}
