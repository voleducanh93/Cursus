using Cursus.Common.Helper;
using Cursus.RepositoryContract.Interfaces;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class WalletHistoryController : ControllerBase
    {
        private readonly IWalletHistoryService _walletHistoryService;
        private readonly APIResponse _response;

        public WalletHistoryController(IWalletHistoryService walletHistoryService, APIResponse response)
        {
            _walletHistoryService = walletHistoryService;
            _response = response;
        }

        /// <summary>
        /// Get all wallet history entries with pagination
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortOrder"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Instructor")]
        public async Task<ActionResult<APIResponse>> GetAllWalletHistories([FromQuery] string? searchTerm,
            [FromQuery] string? sortColumn,
            [FromQuery] string? sortOrder,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var walletHistories = await _walletHistoryService.GetWalletHistoriessAsync(searchTerm, sortColumn, sortOrder, page, pageSize);
            if (walletHistories.Items.Any())
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = walletHistories;
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("No wallet histories found");
            return BadRequest(_response);
        }

        /// <summary>
        /// Get wallet history by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Instructor")]
        public async Task<ActionResult<APIResponse>> GetWalletHistory(int id)
        {
            
                var walletHistoryDto = await _walletHistoryService.GetByIdAsync(id);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = walletHistoryDto;
                return Ok(_response);
           
        }

        /// <summary>
        /// Get wallet histories by wallet ID
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        [HttpGet("byWallet/{walletId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User,Instructor")]
        public async Task<ActionResult<APIResponse>> GetWalletHistoriesByWalletId(int walletId)
        {
            
                var walletHistories = await _walletHistoryService.GetByWalletId(walletId);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = walletHistories;
                return Ok(_response);
            
        }
    }
}
