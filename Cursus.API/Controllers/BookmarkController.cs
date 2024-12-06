using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
    public class BookmarkController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly APIResponse _response;

        public BookmarkController(IBookmarkService bookmarkService, APIResponse response)
        {
            _bookmarkService = bookmarkService;
            _response = response;
        }
        /// <summary>
        /// GetBookMarks
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        // Get bookmarks with sorting functionality
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookmarkDTO>>> GetBookmarks(
            string userId,
            string? sortBy = null,
            string? sortOrder = "asc")
        {
            var bookmarks = await _bookmarkService.GetFilteredAndSortedBookmarksAsync(userId, sortBy, sortOrder);
            return Ok(bookmarks);
        }
        /// <summary>
        /// GetCourseDetails
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        [HttpGet("{courseId}/details")]
        public async Task<ActionResult<CourseDetailDTO>> GetCourseDetails(int courseId)
        {
            var courseDetails = await _bookmarkService.GetCourseDetailsAsync(courseId);
            if (courseDetails == null) return NotFound();
            return Ok(courseDetails);
        }
        /// <summary>
        ///  CreateBookmark
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="courseId"></param>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateBookmark(string userId, int courseId)
        {
            await _bookmarkService.CreateBookmarkAsync(userId, courseId);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Create successfully.";

            return Ok(_response);
        }

    }
}
