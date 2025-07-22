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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Lấy tất cả projects (chỉ admin)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ProjectApiResponse<IEnumerable<ProjectResponse>>>> GetAllProjects()
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

        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<SimpleResponse>> UpdateProjects(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                var project = await _projectService.GetAllProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new SimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy project"
                    });
                }
                
                project.ProjectName = request.ProjectName;
                project.Description = request.Description;
                project.GitHubURL = request.GitHubURL;
                project.ProjectURL = request.ProjectURL;
                project.ThumbnailURL = request.ThumbnailURL;
                project.Technologies = request.Technologies;
                var result = await _projectService.UpdateProjectAsync(project);
                if (result)
                {
                    return Ok(new SimpleResponse
                    {
                        Success = true,
                        Message = "Cập nhật thành công"
                    });
                }
                else
                {
                    return BadRequest(new SimpleResponse
                    {
                        Success = false,
                        Message = "Cập nhật thất bại"
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
                    return Ok(new SimpleResponse
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
        /// Lấy project theo ID (chỉ admin)
        /// </summary>
        // [HttpGet("{id}")]    
        // [AdminAuth]
        // public async Task<ActionResult<ProjectApiResponse<ProjectResponse>>> GetProject(int id)
        // {
        //     try
        //     {
        //         var project = await _projectService.GetAllProjectByIdAsync(id);
        //         if (project == null)
        //         {
        //             return NotFound(new ProjectApiResponse<ProjectResponse>
        //             {
        //                 Success = false,
        //                 Message = "Không tìm thấy project"
        //             });
        //         }

        //         var response = new ProjectResponse
        //         {
        //             ProjectID = project.ProjectID,
        //             ProjectName = project.ProjectName,
        //             Description = project.Description,
        //             GitHubURL = project.GitHubURL,
        //             ProjectURL = project.ProjectURL,
        //             ThumbnailURL = project.ThumbnailURL,
        //             Technologies = project.Technologies,
        //             DisplayOrder = project.DisplayOrder,
        //             IsPublished = project.IsPublished
        //         };

        //         return Ok(new ProjectApiResponse<ProjectResponse>
        //         {
        //             Success = true,
        //             Message = "Lấy thông tin project thành công",
        //             Data = response
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new ProjectApiResponse<ProjectResponse>
        //         {
        //             Success = false,
        //             Message = $"Lỗi: {ex.Message}"
        //         });
        //     }
        // }

        /// <summary>
        /// Lấy tất cả projects (cho guest xem)
        /// </summary>
        // [HttpGet("public")]
        // public async Task<ActionResult<ProjectApiResponse<IEnumerable<ProjectResponse>>>> GetPublicProjects()
        // {
        //     try
        //     {
        //         var projects = await _projectService.GetAllProjectsAsync();
        //         var projectResponses = projects.Where(p => p.IsPublished).Select(p => new ProjectResponse
        //         {       
        //             ProjectID = p.ProjectID,
        //             ProjectName = p.ProjectName,
        //             Description = p.Description,
        //             GitHubURL = p.GitHubURL,
        //             ProjectURL = p.ProjectURL,
        //             ThumbnailURL = p.ThumbnailURL,
        //             Technologies = p.Technologies,
        //             DisplayOrder = p.DisplayOrder,
        //             IsPublished = p.IsPublished
        //         });

        //         return Ok(new ProjectApiResponse<IEnumerable<ProjectResponse>>
        //         {
        //             Success = true,
        //             Message = "Lấy danh sách project thành công",
        //             Data = projectResponses
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new ProjectApiResponse<IEnumerable<ProjectResponse>>
        //         {
        //             Success = false,
        //             Message = $"Lỗi: {ex.Message}"
        //         });
        //     }
        // }

    }
}
