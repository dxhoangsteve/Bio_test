using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Models;
using BioWeb.Server.ViewModels.Requests;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Attributes;
using BioWeb.Server.Services;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly IAboutMeService _aboutMeService;
        private readonly IContactService _contactService;

        public SiteController(IAboutMeService aboutMeService, IContactService contactService)
        {
            _aboutMeService = aboutMeService;
            _contactService = contactService;
        }

        /// <summary>
        /// Lấy thông tin About Me - public
        /// </summary>
        [HttpGet("about")]
        public async Task<ActionResult<SiteApiResponse<AboutMeResponse>>> GetAboutMe()
        {
            try
            {
                var aboutMe = await _aboutMeService.GetAboutMeAsync();
                if (aboutMe == null)
                {
                    return NotFound(new SiteApiResponse<AboutMeResponse>
                    {
                        Success = false,
                        Message = "Chưa có thông tin About Me"
                    });
                }

                var response = new AboutMeResponse
                {
                    FullName = aboutMe.FullName,
                    JobTitle = aboutMe.JobTitle,
                    AvatarURL = aboutMe.AvatarURL,
                    BioSummary = aboutMe.BioSummary
                };

                return Ok(new SiteApiResponse<AboutMeResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin About Me thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteApiResponse<AboutMeResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin About Me - admin
        /// </summary>
        [HttpGet("admin/about")]
        [AdminAuth]
        public async Task<ActionResult<SiteApiResponse<AboutMeAdminResponse>>> GetAboutMeForAdmin()
        {
            try
            {
                var aboutMe = await _aboutMeService.GetAboutMeAsync();
                if (aboutMe == null)
                {
                    return NotFound(new SiteApiResponse<AboutMeAdminResponse>
                    {
                        Success = false,
                        Message = "Chưa có thông tin About Me"
                    });
                }

                var response = new AboutMeAdminResponse
                {
                    AboutMeID = aboutMe.AboutMeID,
                    FullName = aboutMe.FullName,
                    JobTitle = aboutMe.JobTitle,
                    AvatarURL = aboutMe.AvatarURL,
                    BioSummary = aboutMe.BioSummary,
                    UpdatedAt = aboutMe.UpdatedAt
                };

                return Ok(new SiteApiResponse<AboutMeAdminResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin About Me thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteApiResponse<AboutMeAdminResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo/cập nhật About Me - admin
        /// </summary>
        [HttpPost("admin/about")]
        [AdminAuth]
        public async Task<ActionResult<SiteSimpleResponse>> CreateOrUpdateAboutMe([FromBody] CreateAboutMeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new SiteApiResponse<AboutMeAdminResponse>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = errors
                });
            }

            try
            {
                var aboutMe = new AboutMe
                {
                    FullName = request.FullName,
                    JobTitle = request.JobTitle,
                    AvatarURL = request.AvatarURL,
                    BioSummary = request.BioSummary
                };

                await _aboutMeService.CreateOrUpdateAboutMeAsync(aboutMe);

                return Ok(new SiteSimpleResponse
                {
                    Success = true,
                    Message = "Cập nhật About Me thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteSimpleResponse
                {
                    Success = false,
                    Message = $"Cập nhật About Me thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin Contact - public
        /// </summary>
        [HttpGet("contact")]
        public async Task<ActionResult<SiteApiResponse<ContactResponse>>> GetContact()
        {
            try
            {
                var contact = await _contactService.GetContactAsync();
                if (contact == null)
                {
                    return NotFound(new SiteApiResponse<ContactResponse>
                    {
                        Success = false,
                        Message = "Chưa có thông tin Contact"
                    });
                }

                var response = new ContactResponse
                {
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    Address = contact.Address,
                    GitHubURL = contact.GitHubURL,
                    LinkedInURL = contact.LinkedInURL,
                    FacebookURL = contact.FacebookURL
                };

                return Ok(new SiteApiResponse<ContactResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin Contact thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteApiResponse<ContactResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin Contact - admin
        /// </summary>
        [HttpGet("admin/contact")]
        [AdminAuth]
        public async Task<ActionResult<SiteApiResponse<ContactAdminResponse>>> GetContactForAdmin()
        {
            try
            {
                var contact = await _contactService.GetContactAsync();
                if (contact == null)
                {
                    return NotFound(new SiteApiResponse<ContactAdminResponse>
                    {
                        Success = false,
                        Message = "Chưa có thông tin Contact"
                    });
                }

                var response = new ContactAdminResponse
                {
                    ContactID = contact.ContactID,
                    Email = contact.Email,
                    PhoneNumber = contact.PhoneNumber,
                    Address = contact.Address,
                    GitHubURL = contact.GitHubURL,
                    LinkedInURL = contact.LinkedInURL,
                    FacebookURL = contact.FacebookURL,
                    UpdatedAt = contact.UpdatedAt
                };

                return Ok(new SiteApiResponse<ContactAdminResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin Contact thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteApiResponse<ContactAdminResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo/cập nhật Contact - admin
        /// </summary>
        [HttpPost("admin/contact")]
        [AdminAuth]
        public async Task<ActionResult<SiteSimpleResponse>> CreateOrUpdateContact([FromBody] CreateContactRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new SiteApiResponse<ContactAdminResponse>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = errors
                });
            }

            try
            {
                var contact = new Contact
                {
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    GitHubURL = request.GitHubURL,
                    LinkedInURL = request.LinkedInURL,
                    FacebookURL = request.FacebookURL
                };

                await _contactService.CreateOrUpdateContactAsync(contact);

                return Ok(new SiteSimpleResponse
                {
                    Success = true,
                    Message = "Cập nhật Contact thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new SiteSimpleResponse
                {
                    Success = false,
                    Message = $"Cập nhật Contact thất bại: {ex.Message}"
                });
            }
        }
    }
}
