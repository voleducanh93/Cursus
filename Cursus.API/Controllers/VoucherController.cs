using Azure;
using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.Repository.Repository;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using QuestPDF.Infrastructure;
using System.Net;

namespace Cursus.API.Controllers
{
    /// <summary>
    /// Voucher
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly APIResponse _apiResponse;

        /// <summary>
        /// Voucher
        /// </summary>
        /// <param name="voucherService"></param>
        /// <param name="apiResponse"></param>
        public VoucherController(IVoucherService voucherService, APIResponse apiResponse)
        {
            _voucherService = voucherService;
            _apiResponse = apiResponse;
        }

        /// <summary>
        /// Get Voucher
        /// </summary>
        /// <param name="VoucherCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]

        public async Task<IActionResult> GetVouchersByCode(string VoucherCode)
        {
            var vouchers = await _voucherService.GetVoucherByCode(VoucherCode);
            if (vouchers == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.NotFound;
                return StatusCode((int)_apiResponse.StatusCode, _apiResponse);
            }
            _apiResponse.Result = vouchers;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return StatusCode((int)_apiResponse.StatusCode, _apiResponse);
        }

        /// <summary>
        /// Create Voucher
        /// </summary>
        /// <param name="createVoucherDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]

        public async Task<IActionResult> CreateVoucher([FromBody] VoucherDTO createVoucherDTO)
        {
            

            if (!ModelState.IsValid)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(_apiResponse);
            }

            var voucher = await _voucherService.CreateVoucher(createVoucherDTO);
            if (voucher == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages.Add("Voucher already exists");
                return BadRequest(_apiResponse);
            }

            _apiResponse.IsSuccess = true;
            _apiResponse.StatusCode = HttpStatusCode.Created;
            _apiResponse.Result = voucher;

            return CreatedAtAction(nameof(GetVouchersByCode), new { voucherCode = voucher.VoucherCode }, _apiResponse);
        }



        /// <summary>
        /// Update Voucher
        /// </summary>
        /// <param name="Userid"></param>
        /// <param name="updateVoucherDTO"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]

        public async Task<IActionResult> UpdateVoucher(int Userid, [FromBody] VoucherDTO updateVoucherDTO)
        {
            var voucher = await _voucherService.UpdateVoucher(Userid, updateVoucherDTO);
            if (voucher == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }
            _apiResponse.Result = voucher;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(voucher);
        }

        /// <summary>
        /// Delete Voucher
        /// </summary>
        /// <param name="VoucherId"></param>
        /// <returns></returns>
        [HttpDelete("{VoucherId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> DeleteVoucher(int VoucherId)
        {
            var voucher = await _voucherService.DeleteVoucher(VoucherId);
            if (voucher == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }
            _apiResponse.Result = voucher;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(voucher);
        }
        /// <summary>
        /// Receive Voucher
        /// </summary>
        /// <param name="voucherId"></param>
        /// <returns></returns>

        [HttpPost("ReceiveVoucher/{voucherId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> ReceiveVoucher( int voucherId)
        {
            var voucher = await _voucherService.ReceiveVoucher( voucherId);
            if (!voucher)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages.Add("Voucher not found");
                return BadRequest(_apiResponse);
            }

            _apiResponse.Result = voucher;
            _apiResponse.StatusCode = HttpStatusCode.OK;
            return Ok(voucher);
        }

        /// <summary>
        /// Give Voucher
        /// </summary>
        /// <param name="RecieverID"></param>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        [HttpPost("GiveVoucher/{RecieverID}/{voucherId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<IActionResult> GiveVoucher( string RecieverID, int voucherId)
        {
            var voucher = await _voucherService.GiveVoucher( RecieverID, voucherId);
            if(!voucher)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.ErrorMessages.Add("Voucher not found");
                return BadRequest(_apiResponse);
            }
            else
            {
                _apiResponse.Result = voucher;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(voucher);
            }
        }
    }
}
