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
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Lấy tất cả contacts (chỉ admin)
        /// </summary>
        [HttpGet]
        [AdminAuth]
        public async Task<ActionResult<ContactApiResponse<IEnumerable<ContactResponse>>>> GetAllContacts()
        {
            try
            {
                var contacts = await _contactService.GetAllContactsAsync();
                var contactResponses = contacts.Select(c => new ContactResponse
                {
                    ContactID = c.ContactID,
                    FullName = c.FullName,
                    Email = c.Email,
                    Message = c.Message,
                    SentDate = c.SentDate,
                    IsRead = c.IsRead,
                    ReadCount = c.ReadCount
                });

                return Ok(new ContactApiResponse<IEnumerable<ContactResponse>>
                {
                    Success = true,
                    Message = "Lấy danh sách contact thành công",
                    Data = contactResponses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactApiResponse<IEnumerable<ContactResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy contact theo ID (chỉ admin)
        /// </summary>
        [HttpGet("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ContactApiResponse<ContactResponse>>> GetContact(int id)
        {
            try
            {
                var contact = await _contactService.GetContactByIdAsync(id);
                if (contact == null)
                {
                    return NotFound(new ContactApiResponse<ContactResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy contact"
                    });
                }

                var response = new ContactResponse
                {
                    ContactID = contact.ContactID,
                    FullName = contact.FullName,
                    Email = contact.Email,
                    Message = contact.Message,
                    SentDate = contact.SentDate,
                    IsRead = contact.IsRead,
                    ReadCount = contact.ReadCount
                };

                return Ok(new ContactApiResponse<ContactResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin contact thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactApiResponse<ContactResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật contact (chỉ admin)
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ContactSimpleResponse>> UpdateContact(int id, [FromBody] UpdateContactRequest request)
        {
            try
            {
                var contact = await _contactService.GetContactByIdAsync(id);
                if (contact == null)
                {
                    return NotFound(new ContactSimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy contact"
                    });
                }

                // Cập nhật thông tin
                contact.FullName = request.FullName;
                contact.Email = request.Email;

                var result = await _contactService.UpdateContactAsync(contact);
                if (result)
                {
                    return Ok(new ContactSimpleResponse
                    {
                        Success = true,
                        Message = "Cập nhật contact thành công"
                    });
                }
                else
                {
                    return BadRequest(new ContactSimpleResponse
                    {
                        Success = false,
                        Message = "Cập nhật contact thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactSimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa contact (chỉ admin)
        /// </summary>
        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ContactSimpleResponse>> DeleteContact(int id)
        {
            try
            {
                var result = await _contactService.DeleteContactAsync(id);
                if (result)
                {
                    return Ok(new ContactSimpleResponse
                    {
                        Success = true,
                        Message = "Xóa contact thành công"
                    });
                }
                else
                {
                    return NotFound(new ContactSimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy contact"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactSimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }





        /// <summary>
        /// Lấy email contact cho guest (public)
        /// </summary>
        [HttpGet("public/email")]
        public async Task<ActionResult<ContactApiResponse<ContactResponseForQuest>>> GetContactEmail()
        {
            try
            {
                var email = await _contactService.GetContactEmailAsync();
                if (string.IsNullOrEmpty(email))
                {
                    return NotFound(new ContactApiResponse<ContactResponseForQuest>
                    {
                        Success = false,
                        Message = "Chưa có thông tin email contact"
                    });
                }

                var response = new ContactResponseForQuest
                {
                    Email = email
                };

                return Ok(new ContactApiResponse<ContactResponseForQuest>
                {
                    Success = true,
                    Message = "Lấy email contact thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactApiResponse<ContactResponseForQuest>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tăng số lượt đọc contact (public endpoint)
        /// </summary>
        [HttpPut("isReadCount")]
        public async Task<ActionResult<ContactSimpleResponse>> IncrementReadCount()
        {
            try
            {
                var result = await _contactService.IncrementReadCountAsync();

                if (result)
                {
                    return Ok(new ContactSimpleResponse
                    {
                        Success = true,
                        Message = "Tăng read count thành công"
                    });
                }
                else
                {
                    return BadRequest(new ContactSimpleResponse
                    {
                        Success = false,
                        Message = "Tăng read count thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ContactSimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }
    }
}

