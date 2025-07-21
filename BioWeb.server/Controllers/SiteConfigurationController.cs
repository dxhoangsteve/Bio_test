using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BioWeb.Server.Data;
using BioWeb.Server.Models;
using BioWeb.Server.ViewModels.DTOs;
using BioWeb.Server.ViewModels.Requests;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Attributes;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteConfigurationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SiteConfigurationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin cấu hình site (chỉ admin)
        /// </summary>
        [HttpGet]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<SiteConfigurationResponse>>> GetSiteConfiguration()
        {
            var config = await _context.SiteConfigurations.FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(new ApiResponse<SiteConfigurationResponse>
                {
                    Success = false,
                    Message = "Chưa có thông tin cấu hình site"
                });
            }

            var response = new SiteConfigurationResponse
            {
                ConfigID = config.ConfigID,
                FullName = config.FullName,
                JobTitle = config.JobTitle,
                AvatarURL = config.AvatarURL,
                BioSummary = config.BioSummary,
                PhoneNumber = config.PhoneNumber,
                Address = config.Address,
                CV_FilePath = config.CV_FilePath,
                FacebookURL = config.FacebookURL,
                GitHubURL = config.GitHubURL,
                LinkedInURL = config.LinkedInURL,
                UpdatedAt = config.UpdatedAt
            };

            return Ok(new ApiResponse<SiteConfigurationResponse>
            {
                Success = true,
                Message = "Lấy thông tin thành công",
                Data = response
            });
        }

        /// <summary>
        /// Cập nhật thông tin cấu hình site (chỉ admin)
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<SimpleResponse>> UpdateSiteConfiguration(int id, [FromBody] UpdateSiteConfigurationRequest request)
        {
            var config = await _context.SiteConfigurations.FindAsync(id);

            if (config == null)
            {
                return NotFound(new SimpleResponse
                {
                    Success = false,
                    Message = "Không tìm thấy cấu hình site"
                });
            }

            // Cập nhật thông tin
            config.FullName = request.FullName;
            config.JobTitle = request.JobTitle;
            config.AvatarURL = request.AvatarURL;
            config.BioSummary = request.BioSummary;
            config.PhoneNumber = request.PhoneNumber;
            config.Address = request.Address;
            config.CV_FilePath = request.CV_FilePath;
            config.FacebookURL = request.FacebookURL;
            config.GitHubURL = request.GitHubURL;
            config.LinkedInURL = request.LinkedInURL;
            config.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new SimpleResponse
                {
                    Success = true,
                    Message = "Cập nhật thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi cập nhật: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa cấu hình site - reset về mặc định (chỉ admin)
        /// </summary>
        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<SimpleResponse>> DeleteSiteConfiguration(int id)
        {
            var config = await _context.SiteConfigurations.FindAsync(id);

            if (config == null)
            {
                return NotFound(new SimpleResponse
                {
                    Success = false,
                    Message = "Không tìm thấy cấu hình site"
                });
            }

            // Reset về giá trị mặc định thay vì xóa
            config.FullName = "Chưa cập nhật";
            config.JobTitle = "";
            config.AvatarURL = "";
            config.BioSummary = "";
            config.PhoneNumber = "";
            config.Address = "";
            config.CV_FilePath = "";
            config.FacebookURL = "";
            config.GitHubURL = "";
            config.LinkedInURL = "";
            config.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new SimpleResponse
                {
                    Success = true,
                    Message = "Reset thông tin thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi reset: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin public (cho frontend hiển thị)
        /// </summary>
        [HttpGet("public")]
        public async Task<ActionResult<ApiResponse<PublicSiteConfigurationResponse>>> GetPublicInfo()
        {
            var config = await _context.SiteConfigurations.FirstOrDefaultAsync();

            if (config == null)
            {
                return NotFound(new ApiResponse<PublicSiteConfigurationResponse>
                {
                    Success = false,
                    Message = "Chưa có thông tin"
                });
            }

            var publicInfo = new PublicSiteConfigurationResponse
            {
                FullName = config.FullName,
                JobTitle = config.JobTitle,
                AvatarURL = config.AvatarURL,
                BioSummary = config.BioSummary,
                Address = config.Address,
                FacebookURL = config.FacebookURL,
                GitHubURL = config.GitHubURL,
                LinkedInURL = config.LinkedInURL
            };

            return Ok(new ApiResponse<PublicSiteConfigurationResponse>
            {
                Success = true,
                Message = "Lấy thông tin public thành công",
                Data = publicInfo
            });
        }
    }
}
