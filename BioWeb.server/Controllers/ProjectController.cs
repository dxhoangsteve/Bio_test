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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Lấy tất cả projects published (public)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProjectApiResponse<IEnumerable<ProjectResponse>>>> GetPublishedProjects()
        {
            try
            {
                var projects = await _projectService.GetPublishedProjectsAsync();
                var projectResponses = projects.Select(p => new ProjectResponse
                {
                    ProjectID = p.ProjectID,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    GitHubURL = p.GitHubURL,
                    ProjectURL = p.ProjectURL,
                    ThumbnailURL = p.ThumbnailURL,
                    Technologies = p.Technologies,
                    DisplayOrder = p.DisplayOrder,
                    IsPublished = p.IsPublished
                });

                return Ok(new ProjectApiResponse<IEnumerable<ProjectResponse>>
                {
                    Success = true,
                    Message = "Lấy danh sách project thành công",
                    Data = projectResponses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<IEnumerable<ProjectResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy tất cả projects (admin)
        /// </summary>
        [HttpGet("admin")]
        [AdminAuth]
        public async Task<ActionResult<ProjectApiResponse<IEnumerable<ProjectResponse>>>> GetAllProjectsForAdmin()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                var projectResponses = projects.Select(p => new ProjectResponse
                {
                    ProjectID = p.ProjectID,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    GitHubURL = p.GitHubURL,
                    ProjectURL = p.ProjectURL,
                    ThumbnailURL = p.ThumbnailURL,
                    Technologies = p.Technologies,
                    DisplayOrder = p.DisplayOrder,
                    IsPublished = p.IsPublished
                });

                return Ok(new ProjectApiResponse<IEnumerable<ProjectResponse>>
                {
                    Success = true,
                    Message = "Lấy danh sách project thành công",
                    Data = projectResponses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<IEnumerable<ProjectResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy project theo ID (public - chỉ published)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectApiResponse<ProjectResponse>>> GetProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null || !project.IsPublished)
                {
                    return NotFound(new ProjectApiResponse<ProjectResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy project hoặc project chưa được publish"
                    });
                }

                var response = new ProjectResponse
                {
                    ProjectID = project.ProjectID,
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    GitHubURL = project.GitHubURL,
                    ProjectURL = project.ProjectURL,
                    ThumbnailURL = project.ThumbnailURL,
                    Technologies = project.Technologies,
                    DisplayOrder = project.DisplayOrder,
                    IsPublished = project.IsPublished
                };

                return Ok(new ProjectApiResponse<ProjectResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin project thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<ProjectResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy project theo ID (admin - xem được hết)
        /// </summary>
        [HttpGet("admin/{id}")]
        [AdminAuth]
        public async Task<ActionResult<ProjectApiResponse<ProjectResponse>>> GetProjectForAdmin(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ProjectApiResponse<ProjectResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy project"
                    });
                }

                var response = new ProjectResponse
                {
                    ProjectID = project.ProjectID,
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    GitHubURL = project.GitHubURL,
                    ProjectURL = project.ProjectURL,
                    ThumbnailURL = project.ThumbnailURL,
                    Technologies = project.Technologies,
                    DisplayOrder = project.DisplayOrder,
                    IsPublished = project.IsPublished
                };

                return Ok(new ProjectApiResponse<ProjectResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin project thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<ProjectResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }



        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<SimpleResponse>> DeleteProjects(int id)
        {
            try
            {
                var result = await _projectService.DeleteProjectAsync(id);
                if (result)
                {
                    return Ok(new SimpleResponse
                    {
                        Success = true,
                        Message = "Xóa thành công"
                    });
                }
                else
                {
                    return NotFound(new SimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy project"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [AdminAuth]
        public async Task<ActionResult<SimpleResponse>> CreateProject([FromBody] UpdateProjectRequest request)
        {
            // Kiểm tra ModelState validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors)
                });
            }

            try
            {
                var project = new Project
                {
                    ProjectName = request.ProjectName,
                    Description = request.Description,
                    GitHubURL = request.GitHubURL,
                    ProjectURL = request.ProjectURL,
                    ThumbnailURL = request.ThumbnailURL,
                    Technologies = request.Technologies,
                    DisplayOrder = request.DisplayOrder,
                    IsPublished = request.IsPublished
                };
                var result = await _projectService.CreateProjectAsync(project);
                if (result)
                {
                    return Created($"/api/Project/{project.ProjectID}", new SimpleResponse
                    {
                        Success = true,
                        Message = "Tạo thành công"
                    });
                }
                else
                {
                    return BadRequest(new SimpleResponse
                    {
                        Success = false,
                        Message = "Tạo thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo project mới - admin only
        /// </summary>
        [HttpPost]
        [AdminAuth]
        public async Task<ActionResult<ProjectApiResponse<ProjectResponse>>> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                var project = new Project
                {
                    ProjectName = request.ProjectName,
                    Description = request.Description,
                    GitHubURL = request.GitHubURL,
                    ProjectURL = request.ProjectURL,
                    ThumbnailURL = request.ThumbnailURL,
                    Technologies = request.Technologies,
                    DisplayOrder = request.DisplayOrder,
                    IsPublished = request.IsPublished,
                    ViewCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _projectService.CreateProjectAsync(project);

                if (!result)
                {
                    return BadRequest(new ProjectApiResponse<ProjectResponse>
                    {
                        Success = false,
                        Message = "Tạo project thất bại"
                    });
                }

                var response = new ProjectResponse
                {
                    ProjectID = project.ProjectID,
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    GitHubURL = project.GitHubURL,
                    ProjectURL = project.ProjectURL,
                    ThumbnailURL = project.ThumbnailURL,
                    Technologies = project.Technologies,
                    DisplayOrder = project.DisplayOrder,
                    IsPublished = project.IsPublished,
                    ViewCount = project.ViewCount,
                    CreatedAt = project.CreatedAt,
                    UpdatedAt = project.UpdatedAt
                };

                return Ok(new ProjectApiResponse<ProjectResponse>
                {
                    Success = true,
                    Message = "Tạo project thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<ProjectResponse>
                {
                    Success = false,
                    Message = $"Tạo project thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật project - admin only
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ProjectApiResponse<ProjectResponse>>> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ProjectApiResponse<ProjectResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy project"
                    });
                }

                // Cập nhật thông tin
                project.ProjectName = request.ProjectName;
                project.Description = request.Description;
                project.GitHubURL = request.GitHubURL;
                project.ProjectURL = request.ProjectURL;
                project.ThumbnailURL = request.ThumbnailURL;
                project.Technologies = request.Technologies;
                project.DisplayOrder = request.DisplayOrder;
                project.IsPublished = request.IsPublished;

                var result = await _projectService.UpdateProjectAsync(project);

                if (!result)
                {
                    return BadRequest(new ProjectApiResponse<ProjectResponse>
                    {
                        Success = false,
                        Message = "Cập nhật project thất bại"
                    });
                }

                // Lấy lại project sau khi update
                var updatedProject = await _projectService.GetProjectByIdAsync(id);

                var response = new ProjectResponse
                {
                    ProjectID = updatedProject.ProjectID,
                    ProjectName = updatedProject.ProjectName,
                    Description = updatedProject.Description,
                    GitHubURL = updatedProject.GitHubURL,
                    ProjectURL = updatedProject.ProjectURL,
                    ThumbnailURL = updatedProject.ThumbnailURL,
                    Technologies = updatedProject.Technologies,
                    DisplayOrder = updatedProject.DisplayOrder,
                    IsPublished = updatedProject.IsPublished,
                    ViewCount = updatedProject.ViewCount,
                    CreatedAt = updatedProject.CreatedAt
                };

                return Ok(new ProjectApiResponse<ProjectResponse>
                {
                    Success = true,
                    Message = "Cập nhật project thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectApiResponse<ProjectResponse>
                {
                    Success = false,
                    Message = $"Cập nhật project thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa project - admin only
        /// </summary>
        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ProjectSimpleResponse>> DeleteProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new ProjectSimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy project"
                    });
                }

                await _projectService.DeleteProjectAsync(id);

                return Ok(new ProjectSimpleResponse
                {
                    Success = true,
                    Message = "Xóa project thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProjectSimpleResponse
                {
                    Success = false,
                    Message = $"Xóa project thất bại: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// Request model để tạo project
    /// </summary>
    public class CreateProjectRequest
    {
        public string ProjectName { get; set; } = "";
        public string Description { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string ProjectURL { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public string Technologies { get; set; } = "";
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    /// <summary>
    /// Response đơn giản cho Project
    /// </summary>
    public class ProjectSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
