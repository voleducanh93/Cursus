using Cursus.Common.Helper;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly APIResponse _response;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
            _response = new APIResponse();
        }

        /// <summary>
        /// Modify user's status
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        // POST api/admin/toggleuserstatus?userId=someUserId
        [HttpPost("toggle-user-status")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var apiResponse = new APIResponse();

            var result = await _adminService.ToggleUserStatusAsync(userId);
            if (result)
            {
                apiResponse.StatusCode = HttpStatusCode.OK;
                apiResponse.IsSuccess = true;
                apiResponse.Result = "User status has been updated";
            }
            else
            {
                apiResponse.StatusCode = HttpStatusCode.BadRequest;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add("Failed to update user status");
            }


            
            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        // GET api/admin/manageusers
        [HttpGet("manage-users")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var apiResponse = new APIResponse();

            try
            {
                var users = await _adminService.GetAllUser();
                if (users != null && users.Count() > 0)
                {
                    apiResponse.StatusCode = HttpStatusCode.OK;
                    apiResponse.IsSuccess = true;
                    apiResponse.Result = users;
                }
                else
                {
                    apiResponse.StatusCode = HttpStatusCode.NotFound;
                    apiResponse.IsSuccess = false;
                    apiResponse.ErrorMessages.Add("No users found");
                }
            }
            catch (Exception ex)
            {
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add(ex.Message);
            }

            return StatusCode((int)apiResponse.StatusCode, apiResponse);

        }
        /// <summary>
        /// Add comments
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        // POST api/admin/add-comments
        [HttpPost("add-comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> AdminComments([FromQuery]string userId,[FromQuery] string comment)
        {
          
            var result = await _adminService.AdminComments(userId, comment);
            if (result)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = "Comment is sucessful";
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Failed to add comment");
                return BadRequest(_response);
            }
        }
        /// <summary>
        /// Get instructor info
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Authenticate error</response>
        // GET api/admin/get-instructor-info
        [HttpGet("get-instructor-info")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetInformationInstructor([FromQuery] int instructorId)
        {
            var apiResponse = new APIResponse();
            var instructorInfo = await _adminService.GetInformationInstructor(instructorId);

            if (instructorInfo != null && !instructorInfo.ContainsKey("Error"))
            {
                apiResponse.StatusCode = HttpStatusCode.OK;
                apiResponse.IsSuccess = true;
                apiResponse.Result = instructorInfo;
            }
            else
            {
                apiResponse.StatusCode = HttpStatusCode.NotFound;
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages.Add(instructorInfo?["Error"]?.ToString() ?? "Unknown error");
            }

            return StatusCode((int)apiResponse.StatusCode, apiResponse);
        }



        ///// <summary>
        ///// Accept Payout
        ///// </summary>
        ///// <param name="transactionId"></param>
        ///// <returns></returns>
        ///// <response code="401">Authenticate error</response>
        //// POST api/admin/accept-payout
        //[HttpPost("accept-payout")]
        //[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        //public async Task<IActionResult> AcceptPayout([FromQuery] int transactionId)
        //{
        //    await _adminService.AcceptPayout(transactionId);
        //    _response.IsSuccess = true;
        //    _response.StatusCode = HttpStatusCode.OK;
        //    _response.Result = "Payout accepted";
        //    return Ok(_response);
        //}

    }
}

