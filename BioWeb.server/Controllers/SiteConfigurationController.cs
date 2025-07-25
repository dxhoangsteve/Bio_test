using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Models;
using BioWeb.Shared.Models.DTOs;
using BioWeb.Server.ViewModels.Requests;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Attributes;
using BioWeb.Server.Services;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteConfigurationController : ControllerBase
    {
        private readonly ISiteConfigurationService _siteConfigService;

        public SiteConfigurationController(ISiteConfigurationService siteConfigService)
        {
            _siteConfigService = siteConfigService;
        }

        /// <summary>
        /// get data riêng cho admin
        /// </summary>
        [HttpGet]
        [AdminAuth]
        public async Task<ActionResult<SiteConfigurationApiResponse<SiteConfigurationResponse>>> GetSiteConfiguration()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

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
                    ViewCount = config.ViewCount,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new SiteConfigurationApiResponse<SiteConfigurationResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<SiteConfigurationResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật tt admin
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<SiteConfigurationSimpleResponse>> UpdateSiteConfiguration(int id, [FromBody] UpdateSiteConfigurationRequest request)
        {
            try
            {
                var config = await _siteConfigService.GetSiteConfigurationAsync();

                if (config == null || config.ConfigID != id)
                {
                    return NotFound(new SiteConfigurationSimpleResponse
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

                var result = await _siteConfigService.UpdateSiteConfigurationAsync(config);
                if (result)
                {
                    return Ok(new SiteConfigurationSimpleResponse
                    {
                        Success = true,
                        Message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return BadRequest(new SiteConfigurationSimpleResponse
                    {
                        Success = false,
                        Message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationSimpleResponse
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
        public async Task<ActionResult<SiteConfigurationSimpleResponse>> DeleteSiteConfiguration(int id)
        {
            try
            {
                var result = await _siteConfigService.ResetSiteConfigurationAsync(id);
                if (result)
                {
                    return Ok(new SiteConfigurationSimpleResponse
                    {
                        Success = true,
                        Message = "Reset thông tin thành công"
                    });
                }
                else
                {
                    return NotFound(new SiteConfigurationSimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy cấu hình site"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationSimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi reset: {ex.Message}"
                });
            }
        }



        /// <summary>
        /// Lấy thông tin Contact - liên hệ (public)
        /// </summary>
        [HttpGet("contact")]
        public async Task<ActionResult<SiteConfigurationApiResponse<ContactResponse>>> GetContactInfo()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                var contactInfo = new ContactResponse
                {
                    Email = config.Email,
                    PhoneNumber = config.PhoneNumber,
                    Address = config.Address,
                    GitHubURL = config.GitHubURL,
                    LinkedInURL = config.LinkedInURL,
                    FacebookURL = config.FacebookURL
                };

                return Ok(new SiteConfigurationApiResponse<ContactResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin Contact thành công",
                    Data = contactInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<ContactResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin public của site và tăng view count (public)
        /// </summary>
        [HttpGet("public")]
        public async Task<ActionResult<SiteConfigurationApiResponse<SiteConfigurationDto>>> GetPublicSiteInfo()
        {
            try
            {
                // Lấy IP của client
                var clientIp = GetClientIpAddress();

                // Tăng view count với IP tracking
                await _siteConfigService.IncrementViewCountAsync(clientIp);

                // Lấy thông tin site
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                var siteInfo = new SiteConfigurationDto
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
                    ViewCount = config.ViewCount,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new SiteConfigurationApiResponse<SiteConfigurationDto>
                {
                    Success = true,
                    Message = "Lấy thông tin site thành công",
                    Data = siteInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<SiteConfigurationDto>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }



        /// <summary>
        /// Lấy thông tin About Me từ SiteConfiguration - public
        /// </summary>
        [HttpGet("about-me")]
        public async Task<ActionResult<SiteConfigurationApiResponse<AboutMeDto>>> GetAboutMe()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                var aboutMe = new AboutMeDto
                {
                    AboutMeID = config.ConfigID, // Sử dụng ConfigID làm AboutMeID
                    FullName = config.FullName,
                    JobTitle = config.JobTitle,
                    AvatarURL = config.AvatarURL,
                    BioSummary = config.BioSummary,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new SiteConfigurationApiResponse<AboutMeDto>
                {
                    Success = true,
                    Message = "Lấy thông tin About Me thành công",
                    Data = aboutMe
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<AboutMeDto>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật About Me - admin only
        /// </summary>
        [HttpPut("about-me")]
        [AdminAuth]
        public async Task<ActionResult<SiteConfigurationApiResponse<AboutMeDto>>> UpdateAboutMe([FromBody] UpdateAboutMeRequest request)
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                // Chỉ cập nhật các field About Me
                config.FullName = request.FullName;
                config.JobTitle = request.JobTitle;
                config.BioSummary = request.BioSummary;
                config.UpdatedAt = DateTime.UtcNow;

                await _siteConfigService.UpdateSiteConfigurationAsync(config);

                var aboutMe = new AboutMeDto
                {
                    AboutMeID = config.ConfigID,
                    FullName = config.FullName,
                    JobTitle = config.JobTitle,
                    AvatarURL = config.AvatarURL,
                    BioSummary = config.BioSummary,
                    UpdatedAt = config.UpdatedAt
                };

                return Ok(new SiteConfigurationApiResponse<AboutMeDto>
                {
                    Success = true,
                    Message = "Cập nhật About Me thành công",
                    Data = aboutMe
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<AboutMeDto>
                {
                    Success = false,
                    Message = $"Cập nhật About Me thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật Contact - admin only
        /// </summary>
        [HttpPut("contact")]
        [AdminAuth]
        public async Task<ActionResult<SiteConfigurationApiResponse<ContactResponse>>> UpdateContact([FromBody] UpdateContactRequest request)
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                // Cập nhật các field Contact
                config.PhoneNumber = request.PhoneNumber;
                config.Address = request.Address;
                config.FacebookURL = request.FacebookURL;
                config.GitHubURL = request.GitHubURL;
                config.LinkedInURL = request.LinkedInURL;
                config.UpdatedAt = DateTime.UtcNow;

                await _siteConfigService.UpdateSiteConfigurationAsync(config);

                var contact = new ContactResponse
                {
                    Email = config.Email,
                    PhoneNumber = config.PhoneNumber,
                    Address = config.Address,
                    GitHubURL = config.GitHubURL,
                    LinkedInURL = config.LinkedInURL,
                    FacebookURL = config.FacebookURL
                };

                return Ok(new SiteConfigurationApiResponse<ContactResponse>
                {
                    Success = true,
                    Message = "Cập nhật Contact thành công",
                    Data = contact
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<ContactResponse>
                {
                    Success = false,
                    Message = $"Cập nhật Contact thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tăng view count cho bio - public
        /// </summary>
        [HttpPut("contact/view-count")]
        public async Task<ActionResult<SiteSimpleResponse>> IncrementViewCount()
        {
            try
            {
                // Lấy IP của client
                var clientIp = GetClientIpAddress();

                // Tăng view count với IP tracking
                var success = await _siteConfigService.IncrementViewCountAsync(clientIp);

                return Ok(new SiteSimpleResponse
                {
                    Success = true,
                    Message = success ? "Đã tăng view count" : "View count không thay đổi (cooldown)"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteSimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy IP address của client
        /// </summary>
        private string GetClientIpAddress()
        {
            // Kiểm tra X-Forwarded-For header (cho proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Kiểm tra X-Real-IP header
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback về RemoteIpAddress
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

    }

    /// <summary>
    /// Request model để cập nhật About Me
    /// </summary>
    public class UpdateAboutMeRequest
    {
        public string FullName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string BioSummary { get; set; } = "";
    }

    /// <summary>
    /// Request model để cập nhật Contact
    /// </summary>
    public class UpdateContactRequest
    {
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string FacebookURL { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
    }

    /// <summary>
    /// Response đơn giản
    /// </summary>
    public class SiteSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
