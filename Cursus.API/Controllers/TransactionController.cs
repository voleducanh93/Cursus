namespace Demo_PayPal.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Cursus.Common.Helper;  // Use APIResponse from Helper
    using System.Net;
    using System.Threading.Tasks;
    using Cursus.ServiceContract.Interfaces;
    using Cursus.Data.DTO.Payment;
    using Cursus.Data.DTO;
    using Microsoft.AspNetCore.Authorization;

    [Route("api/[controller]")]
    [ApiController]

    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IArchivedTransactionService _archivedTransactionService;
        private readonly APIResponse _response;

        public TransactionController(ITransactionService transactionService, APIResponse response, IArchivedTransactionService archivedTransactionService)
        {
            _transactionService = transactionService;
            _response = response;
            _archivedTransactionService = archivedTransactionService;
        }

        /// <summary>
        /// Get all transactions
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("transaction")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions(int page = 1, int pageSize = 20)
        {
            var transactions = await _transactionService.GetListTransaction(page, pageSize);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transactions;
            return Ok(_response);
        }

        /// <summary>
        /// Get transaction by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("user/{userId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetTransactionsByUserId(string userId, int page = 1, int pageSize = 20)
        {
            var transactions = await _transactionService.GetListTransactionByUserId(userId, page, pageSize);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transactions;
            return Ok(_response);
        }


        /// <summary>
        /// Get pending payout request
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-pending-payout")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetPendingPayoutRequest()
        {
            var transaction = await _transactionService.GetAllPendingPayOutRequest();
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transaction;
            return Ok(_response);
        }

        /// <summary>
        /// Archive transaction
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [HttpGet("archive-transaction/{transactionId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> ArchiveTransaction (int transactionId)
        {
            var transaction = await _archivedTransactionService.ArchiveTransaction(transactionId);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transaction;
            return Ok(_response);
        }

        /// <summary>
        /// Get all archived transaction
        /// </summary>
        /// <returns></returns>
        [HttpGet("archive-transaction")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetAllArchivedTransactions()
        {
            var transactions = await _archivedTransactionService.GetAllArchivedTransactions();
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transactions;
            return Ok(_response);
        }

        /// <summary>
        /// Unarchive transaction
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [HttpGet("unarchive-transaction/{transactionId}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> UnarchiveTransaction(int transactionId)
        {
            var transaction = await _archivedTransactionService.UnarchiveTransaction(transactionId);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = transaction;
            return Ok(_response);
        }
    }
}

