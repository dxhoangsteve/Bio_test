using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Models;
using BioWeb.Server.ViewModels.DTOs;
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
        /// Lấy thông tin public (cho frontend hiển thị)
        /// </summary>
        [HttpGet("public")]
        public async Task<ActionResult<SiteConfigurationApiResponse<PublicSiteConfigurationResponse>>> GetPublicInfo()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

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

                return Ok(new SiteConfigurationApiResponse<PublicSiteConfigurationResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin public thành công",
                    Data = publicInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<PublicSiteConfigurationResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin About Me - Bio summary (public)
        /// </summary>
        [HttpGet("about-me")]
        public async Task<ActionResult<SiteConfigurationApiResponse<AboutMeResponse>>> GetAboutMe()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                var aboutMe = new AboutMeResponse
                {
                    FullName = config.FullName,
                    JobTitle = config.JobTitle,
                    AvatarURL = config.AvatarURL,
                    BioSummary = config.BioSummary
                };

                return Ok(new SiteConfigurationApiResponse<AboutMeResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin About Me thành công",
                    Data = aboutMe
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<AboutMeResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin Contact - liên hệ (public)
        /// </summary>
        [HttpGet("contact")]
        public async Task<ActionResult<SiteConfigurationApiResponse<ContactInfoResponse>>> GetContactInfo()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                var contactInfo = new ContactInfoResponse
                {
                    Email = config.Email,
                    PhoneNumber = config.PhoneNumber,
                    Address = config.Address,
                    GitHubURL = config.GitHubURL,
                    LinkedInURL = config.LinkedInURL,
                    FacebookURL = config.FacebookURL
                };

                return Ok(new SiteConfigurationApiResponse<ContactInfoResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin Contact thành công",
                    Data = contactInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteConfigurationApiResponse<ContactInfoResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

    }
}
