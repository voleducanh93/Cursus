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
    public class PrivacyPolicyController : ControllerBase
    {
        private readonly IPrivacyPolicyService _privacyPolicyService;
        private readonly APIResponse _response;

        public PrivacyPolicyController(IPrivacyPolicyService privacyPolicyService, APIResponse response)
        {
            _privacyPolicyService = privacyPolicyService;
            _response = response;
        }

        /// <summary>
        /// Get all privacy policies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetPrivacyPolicies()
        {
            var privacyPolicies = await _privacyPolicyService.GetPrivacyPolicyAsync();
            if (privacyPolicies.Any())
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = privacyPolicies;
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.ErrorMessages.Add("No privacy policies found.");
            return NotFound(_response);
        }

        /// <summary>
        /// Create a new privacy policy
        /// </summary>
        /// <param name="privacyPolicyDTO"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<APIResponse>> CreatePrivacyPolicy([FromBody] PrivacyPolicyDTO privacyPolicyDTO)
        {
            var createdPrivacyPolicy = await _privacyPolicyService.CreatePrivacyPolicyAsync(privacyPolicyDTO);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = createdPrivacyPolicy;
            return Ok(_response);
        }

        /// <summary>
        /// Update an existing privacy policy
        /// </summary>
        /// <param name="id"></param>
        /// <param name="privacyPolicyDTO"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<APIResponse>> UpdatePrivacyPolicy(int id, [FromBody] PrivacyPolicyDTO privacyPolicyDTO)
        {
            var updatedPrivacyPolicy = await _privacyPolicyService.UpdatePrivacyPolicyAsync(id, privacyPolicyDTO);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = updatedPrivacyPolicy;
            return Ok(_response);
        }

        /// <summary>
        /// Delete a privacy policy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<APIResponse>> DeletePrivacyPolicy(int id)
        {
            await _privacyPolicyService.DeletePrivacyPolicyAsync(id);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.NoContent;
            return NoContent();
        }
    }
}
