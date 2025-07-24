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
            var logger = context.HttpContext.RequestServices.GetService<ILogger<AdminAuthAttribute>>();

            // Kiểm tra JWT token trước
            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            logger?.LogInformation($"AdminAuth: Authorization header = {authHeader}");

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();
                logger?.LogInformation($"AdminAuth: Validating token...");

                if (tokenService != null && tokenService.ValidateToken(token))
                {
                    var principal = tokenService.GetPrincipalFromToken(token);
                    if (principal != null && principal.IsInRole("Admin"))
                    {
                        logger?.LogInformation("AdminAuth: Token valid, user is admin");
                        context.HttpContext.User = principal;
                        await next();
                        return;
                    }
                    else
                    {
                        logger?.LogWarning("AdminAuth: Token valid but user is not admin");
                    }
                }
                else
                {
                    logger?.LogWarning("AdminAuth: Token validation failed");
                }
            }

            // Fallback: Kiểm tra username/password từ header (cho compatibility)
            var username = context.HttpContext.Request.Headers["X-Admin-Username"].FirstOrDefault();
            var password = context.HttpContext.Request.Headers["X-Admin-Password"].FirstOrDefault();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // Validate admin credentials
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var isValid = await authService.IsValidAdminAsync(username, password);

                if (isValid)
                {
                    // Lưu admin info vào HttpContext để dùng trong controller
                    var admin = await authService.GetAdminByUsernameAsync(username);
                    context.HttpContext.Items["CurrentAdmin"] = admin;
                    await next();
                    return;
                }
            }

            // Không có quyền
            logger?.LogWarning("AdminAuth: Access denied - no valid authentication");
            context.Result = new UnauthorizedObjectResult(new SimpleResponse
            {
                Success = false,
                Message = "Cần đăng nhập admin hoặc token không hợp lệ."
            });
        }
    }
}
