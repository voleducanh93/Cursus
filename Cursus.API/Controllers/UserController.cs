using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.RepositoryContract.Interfaces;
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
    public class UserController : ControllerBase

    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly APIResponse _response;
        public UserController(IUserService userService, APIResponse response, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _response = response;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Update user's profile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Update{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> UpdateUserProfile(string id, [FromBody] UserProfileUpdateDTO request)
        {
            var result = await _userService.UpdateUserProfile(id, request);
            if (result != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = result;
                return Ok(_response);
            }
            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Cant Update");
            return BadRequest(_response);
        }


    }
}
