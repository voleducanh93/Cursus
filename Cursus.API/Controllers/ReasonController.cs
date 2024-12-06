using Cursus.RepositoryContract.Interfaces;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cursus.Service.Services;
using Humanizer;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReasonController : ControllerBase
    {
        private readonly IReasonService _reasonService;
        private readonly APIResponse _response;

        public ReasonController(IReasonService reasonService, APIResponse response)
        {
            _reasonService = reasonService;
            _response = response;
        }

        /// <summary>
        /// Create reason
        /// </summary>
        /// <param name="createReasonDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,User,Instructor")]
        public async Task<ActionResult<APIResponse>> CreateReason([FromBody] CreateReasonDTO createReasonDTO)
        {
            var reason = await _reasonService.CreateReason(createReasonDTO);
            _response.Result = reason;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        /// <summary>
        /// Get reason by id
        /// </summary>
        /// <param name="reasonId"></param>
        /// <returns></returns>
        [HttpGet("{reasonId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,User,Instructor")]
        public async Task<ActionResult<IEnumerable<ReasonDTO>>> GetReason(int reasonId)
        {
            var reason = await _reasonService.GetReasonByIdAsync(reasonId);
            _response.Result = reason;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        /// <summary>
        /// Delete reson
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> DeleteReason(int id)
        {
            await _reasonService.DeleteReasonAsync(id);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.NoContent;
            return NoContent();
        }

    }
}
