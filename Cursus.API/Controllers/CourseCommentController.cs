using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]

    public class CourseCommentController : ControllerBase
    {
        private readonly ICourseCommentService _courseCommentService;
        private readonly APIResponse _response;

        public CourseCommentController(ICourseCommentService courseCommentService, APIResponse response)
        {
            _courseCommentService = courseCommentService;
            _response = response;
        }

        /// <summary>
        /// Comment on a course
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("comment-courses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin,Instructor")]
        public async Task<ActionResult<APIResponse>> PostComment([FromBody] CourseCommentCreateDTO dto)
        {

            var comment = await _courseCommentService.PostComment(dto);
            _response.Result = comment;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);

        }

        /// <summary>
        /// Get all comment of a course
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("comment-courses/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin,Instructor")]
        public async Task<ActionResult<APIResponse>> GetCourseComment(int id)
        {
            var comments = await _courseCommentService.GetCourseCommentsAsync(id);
            _response.Result = comments;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        /// <summary>
        /// Delete comment of a course
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("comment-courses/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Admin,Instructor")]
        public async Task<ActionResult<APIResponse>> DeleteComment(int id)
        {
            var comment = await _courseCommentService.DeleteComment(id);
            _response.Result = comment;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}
