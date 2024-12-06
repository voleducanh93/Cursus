namespace Demo_PayPal.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Cursus.Common.Helper;  // Use APIResponse from Helper
    using System.Net;
    using System.Threading.Tasks;
    using Cursus.ServiceContract.Interfaces;
    using Cursus.Data.DTO.Payment;
    using Microsoft.AspNetCore.Authorization;

    [Route("api/[controller]")]
    [ApiController]

    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly APIResponse _response;

        public PaymentController(IPaymentService paymentService, APIResponse response)
        {
            _paymentService = paymentService;
            _response = response;
        }

        /// <summary>
        /// Creates a payment request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create-payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")]
        public async Task<ActionResult<APIResponse>> CreatePayment([FromQuery] CreatePaymentRequest request)
        {
            // Create payment and retrieve approval URL
            var approvalUrl = await _paymentService.CreatePaymentOrder(
                request.OrderId);

            // Build successful response
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = new { ApprovalUrl = approvalUrl };

            return Ok(_response);
        }

        /// <summary>
        /// Captures a payment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("capture-payment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> CapturePayment([FromQuery] CapturePaymentRequest request)
        {
            // Capture the payment and retrieve transaction details
            var transaction = await _paymentService.CapturePayment(
                request.Token,
                request.PayId
                );

            // Build successful response          
            if (transaction.Status == Cursus.Data.Enums.TransactionStatus.Completed)
            {
                
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = new { Message = "Payment successful", Transaction = transaction };
                return Ok(_response);
            }
            else if (transaction.Status == Cursus.Data.Enums.TransactionStatus.Failed)
            {
                
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = new { Message = "Transaction failed, payment was canceled.", Transaction = transaction };
                return Ok(_response);
            }

            return Ok(_response);
        }
    }
}
