using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BioWeb.Server.Services;
using BioWeb.Server.ViewModels.Responses;

namespace BioWeb.Server.Attributes
{
    /// <summary>
    /// Attribute để check admin authentication
    /// </summary>
    public class AdminAuthAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Lấy username và password từ header
            var username = context.HttpContext.Request.Headers["X-Admin-Username"].FirstOrDefault();
            var password = context.HttpContext.Request.Headers["X-Admin-Password"].FirstOrDefault();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.Result = new UnauthorizedObjectResult(new SimpleResponse
                {
                    Success = false,
                    Message = "Cần đăng nhập admin."
                });
                return;
            }

            // Validate admin credentials
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var isValid = await authService.IsValidAdminAsync(username, password);

            if (!isValid)
            {
                context.Result = new UnauthorizedObjectResult(new SimpleResponse
                {
                    Success = false,
                    Message = "Username hoặc password admin không đúng."
                });
                return;
            }

            // Lưu admin info vào HttpContext để dùng trong controller
            var admin = await authService.GetAdminByUsernameAsync(username);
            context.HttpContext.Items["CurrentAdmin"] = admin;

            await next();
        }
    }
}
