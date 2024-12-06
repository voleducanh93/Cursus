using Cursus.Common.Helper;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PayoutRequestController : ControllerBase
    {
        public readonly IPayoutRequestService _payoutRequestService;
        public readonly APIResponse _response;

        public PayoutRequestController(IPayoutRequestService payoutRequestService)
        {
            _payoutRequestService = payoutRequestService;
            _response = new APIResponse();
        }

        [HttpGet("pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetPendingPayoutRequests()
        {
            var pendingRequests = await _payoutRequestService.GetPendingPayoutRequest();
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = pendingRequests;
            return Ok(_response);
        }


        [HttpGet("approved")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetApprovedPayoutRequest()
        {
            var approvedRequest = await _payoutRequestService.GetApprovedPayoutRequest();
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = approvedRequest;
            return Ok(_response);
        }

        [HttpGet("reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetRejectedPayoutRequest()
        {
            var rejectRequest = await _payoutRequestService.GetRejectPayoutRequest();
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = rejectRequest;
            return Ok(_response);
        }
        [HttpPut("approve/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> ApprovePayoutRequest(int id)
        {
            var response = await _payoutRequestService.AcceptPayout(id);
            if (response != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = response;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
        [HttpPut("deny/{id}&{reason}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> DenyPayoutRequest(int id, string reason)
        {
            var response = await _payoutRequestService.DenyPayout(id,reason);
            if (response != null)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = response;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }
    }
}
