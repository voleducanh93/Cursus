using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermPolicyController : ControllerBase
    {
        private readonly ITermPolicyService _termPolicyService;
        private readonly APIResponse _response;

        public TermPolicyController(ITermPolicyService termPolicyService, APIResponse response)
        {
            _termPolicyService = termPolicyService;
            _response = response;
        }

        /// <summary>
        /// Get all term policies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetTermPolicies()
        {
            var termPolicies = await _termPolicyService.GetTermPolicyAsync();
            if (termPolicies != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = termPolicies;
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.ErrorMessages.Add("No term policies found.");
            return NotFound(_response);
        }

        /// <summary>
        /// Create a new term policy
        /// </summary>
        /// <param name="termPolicyDTO"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreateTermPolicy([FromBody] TermPolicyDTO termPolicyDTO)
        {
            var createdTermPolicy = await _termPolicyService.CreateTermPolicyAsync(termPolicyDTO);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = createdTermPolicy;
            return Ok(_response);
        }

        /// <summary>
        /// Update an existing term policy
        /// </summary>
        /// <param name="id"></param>
        /// <param name="termPolicyDTO"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdateTermPolicy(int id, [FromBody] TermPolicyDTO termPolicyDTO)
        {
            var updatedTermPolicy = await _termPolicyService.UpdateTermPolicyAsync(id, termPolicyDTO);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = updatedTermPolicy;
            return Ok(_response);
        }

        /// <summary>
        /// Delete a term policy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeleteTermPolicy(int id)
        {
            await _termPolicyService.DeleteTermPolicyAsync(id);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.NoContent;
            return NoContent();
        }
    }
}
