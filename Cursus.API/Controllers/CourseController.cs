using Azure;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]

    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly APIResponse _response;
        private readonly ICourseProgressService _courseProgressService;

        public CourseController(ICourseService courseService, APIResponse response, ICourseProgressService courseProgressService)

        {
            _courseService = courseService;
            _response = response;
            _courseProgressService = courseProgressService;
        }

		/// <summary>
		/// Create course
		/// </summary>
		/// <param name="courseCreateDTO"></param>
		/// <returns></returns>
		[HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> CreateCourse(CourseCreateDTO courseCreateDTO)
        {
            var createdCourse = await _courseService.CreateCourseWithSteps(courseCreateDTO);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = createdCourse;
            return Ok(_response);
        }

		/// <summary>
		/// Update course
		/// </summary>
		/// <param name="id"></param>
		/// <param name="courseUpdateDTO"></param>
		/// <returns></returns>
		[HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> UpdateCourse(int id, [FromBody] CourseUpdateDTO courseUpdateDTO)
        {
            if (id != courseUpdateDTO.Id)
            {
                return BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = { "Course ID mismatch." }
                });
            }

            try
            {
                var updatedCourse = await _courseService.UpdateCourse(courseUpdateDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = updatedCourse;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(e.Message);
                return BadRequest(_response);
            }
        }


        /// <summary>
        /// Detele course
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> DeleteCourse(int id)
        {
            try
            {
                bool result = await _courseService.DeleteCourse(id);

                if (!result)
                {
                    return NotFound(new APIResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.NotFound,
                        ErrorMessages = { "Course not found." }
                    });
                }

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return NoContent();
            }
            catch (Exception e)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(e.Message);
                return BadRequest(_response);
            }
        }

        /// <summary>
        /// Get all courses with pagination
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortOrder"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,Instructor,User")]
        public async Task<ActionResult<APIResponse>> GetAllCourses([FromQuery] string? searchTerm,
        [FromQuery] string? sortColumn,
        [FromQuery] string? sortOrder,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            try
            {

                var result = await _courseService.GetCoursesAsync(
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    page: page,
                    pageSize: pageSize
                );
                if (result.Items.Any())
                {

                    _response.IsSuccess = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = result;
                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("No courses found");
                    return BadRequest(_response);
                }

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        /// <summary>
        /// Get course by user's ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("courses/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,Admin,User")]
        public async Task<ActionResult<APIResponse>> GetCoursesByUserId(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {

                var courses = await _courseService.GetRegisteredCoursesByUserIdAsync(userId, page, pageSize);

                if (courses != null && courses.Items.Any())
                {
                    _response.IsSuccess = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Result = courses;
                    return Ok(_response);
                }

                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("No courses found for the specified user");
                return NotFound(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        /// <summary>
        /// Get course by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,Admin,User")]
        public async Task<ActionResult<APIResponse>> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = course;
            return Ok(_response);
        }
        /// <summary>
        /// Approve Course
        /// </summary>
        /// <param name="id"></param>
        /// <param choice ="choice"></param>
        /// <returns></returns>
        [HttpGet("ApproveCourse{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> ApproveCourse(int id , bool choice)
        {
            
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = await _courseService.CourseApproval(id, choice);
            return Ok(_response);
        }

        ///// <summary>
        ///// Track progress of a course for a specific user
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="courseId"></param>
        ///// <returns></returns>
        //[HttpGet("TrackProgress")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        //public async Task<ActionResult<APIResponse>> TrackProgress(string userId, int courseId)
        //{
        //    try
        //    {
        //        var progressPercentage = await _courseProgressService.TrackingProgressAsync(userId, courseId);

        //        if (double.IsNaN(progressPercentage))
        //        {
        //            _response.IsSuccess = false;
        //            _response.StatusCode = HttpStatusCode.NotFound;
        //            _response.ErrorMessages.Add("Course or user not found.");
        //            return NotFound(_response);
        //        }

        //        _response.IsSuccess = true;
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.Result = progressPercentage;
        //        return Ok(_response);
        //    }
        //    catch (DivideByZeroException)
        //    {
        //        _response.IsSuccess = false;
        //        _response.StatusCode = HttpStatusCode.BadRequest;
        //        _response.ErrorMessages.Add("No progress found for this course.");
        //        return BadRequest(_response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.StatusCode = HttpStatusCode.InternalServerError;
        //        _response.ErrorMessages.Add($"An error occurred: {ex.Message}");
        //        return StatusCode(StatusCodes.Status500InternalServerError, _response);
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseUpdateStatusDTO"></param>
        /// <returns></returns>
        [HttpPut("UpdateStatus")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> UpdateCourseStatus([FromBody] CourseUpdateStatusDTO courseUpdateStatusDTO)
        {
            var result = await _courseService.UpdateCourseStatus(courseUpdateStatusDTO);

            _response.IsSuccess = result.IsSuccess;
            _response.StatusCode = result.StatusCode;
            _response.ErrorMessages = result.ErrorMessages;

            return StatusCode((int)_response.StatusCode, _response);
        }


        [HttpPost("calculate-earnings")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor,Admin")]
        public async Task<ActionResult<APIResponse>> CalculatePotential([FromBody] CalculateEarningRequestDTO request)
        {
            var earnings = await _courseService.CaculatePotentialEarnings(request.CourseId, request.Months);
            if (earnings == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("Course ID must be greater than 0.");
                return _response;
            }
            if (request.Months <= 0)
            {
                request.Months = 1; // Đặt giá trị mặc định cho months là 1 nếu người dùng nhập 0 hoặc âm
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = earnings;
            return _response;

        }

    }
}
