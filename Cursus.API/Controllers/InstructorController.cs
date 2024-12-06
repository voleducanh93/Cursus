using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.RepositoryContract.Interfaces;
using Cursus.Service.Services;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]


    public class InstructorController : ControllerBase
    {
        private readonly IInstructorService _instructorService;
        private readonly APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;
        private readonly IWalletService _walletService;

        public InstructorController(IInstructorService instructorService, APIResponse aPIResponse, IAuthService authService, UserManager<ApplicationUser> userManager, IEmailService emailService, IWalletService walletService)
        {
            _instructorService = instructorService;
            _response = aPIResponse;
            _authService = authService;
            _userManager = userManager;
            _emailService = emailService;
            _walletService = walletService;
        }

        /// <summary>
        /// Register for instructor
        /// </summary>
        /// <param name="registerInstructorDTO"></param>
        /// <returns></returns>
        [HttpPost("register-instructor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> RegisterInstructor(RegisterInstructorDTO registerInstructorDTO)
        {
            if (!ModelState.IsValid)
            {
                var errorDetails = new Dictionary<string, string>();

                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    var errors = state.Errors.Select(e => e.ErrorMessage).ToList();

                    if (errors.Any())
                    {
                        errorDetails.Add(key, string.Join(", ", errors));
                    }
                }

                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Result = errorDetails;
                return BadRequest(_response);
            }
            var existingUser = await _userManager.FindByEmailAsync(registerInstructorDTO.UserName);
            if (existingUser != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Result = "An account with this email already exists.";
                return BadRequest(_response);
            }
            var result = await _instructorService.InstructorAsync(registerInstructorDTO);

            if (result != null)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(result);
                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Instructor",
                    new { token = token, username = result.UserName },
                    Request.Scheme);
                _emailService.SendEmailConfirmation(result.UserName, confirmationLink);

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = "Instructor registered successfully, please confirm your email";
                return CreatedAtAction(nameof(RegisterInstructor), _response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Result = "Failed to register instructor";
            return BadRequest(_response);
        }

        /// <summary>
        /// Approve instructor
        /// </summary>
        /// <param name="instructorId"></param>
        /// <returns></returns>
        [HttpPost("approve")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> ApproveInstructor([FromQuery] int instructorId)
        {
            var result = await _instructorService.ApproveInstructorAsync(instructorId);
            if (result)
            {

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = "Instructor approved successfully";
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Failed to approve instructor");
            return BadRequest(_response);
        }
        /// <summary>
        /// Reject Instuctor
        /// </summary>
        /// <param name="instructorId"></param>
        /// <returns></returns>
        [HttpPost("reject")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> RejectInstructor([FromQuery] int instructorId)
        {
            var result = await _instructorService.RejectInstructorAsync(instructorId);
            if (result)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = "Instructor rejected successfully";
                return Ok(_response);
            }

            _response.IsSuccess = false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Failed to reject instructor");
            return BadRequest(_response);
        }


        /// <summary>
        /// Confirm email
        /// </summary>
        /// <param name="token"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> ConfirmEmail([FromQuery] string token, [FromQuery] string username)
        {
            try
            {
                var result = await _authService.ConfirmEmail(username, token);
                if (result)
                {
                    _response.IsSuccess = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Can not confirm your email");
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            catch (Exception e)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(e.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }

        /// <summary>
        /// Get instructor courses with earnings
        /// </summary>
        /// <param name="instructorId"></param>
        /// <returns></returns>
        [HttpGet("instructor-courses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,User,Instructor")]
        public async Task<ActionResult<APIResponse>> GetInstructorCourses(int instructorId)
        {
            APIResponse _response = new APIResponse();

            try
            {
                // Gọi service để lấy thông tin các khóa học của giảng viên
                var courseSummary = await _instructorService.GetTotalAmountAsync(instructorId);

                if (courseSummary == null )
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Result = "No courses found for this instructor.";
                    return NotFound(_response);
                }

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = courseSummary;
                return Ok(_response);
            }
            catch (KeyNotFoundException ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Result = ex.Message;
                return NotFound(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Result = $"An unexpected error occurred: {ex.Message}";
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }
        /// <summary>
        /// List all instructors along with user and instructor information
        /// </summary>
        /// <returns></returns>
        [HttpGet("list-instructors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,User")]
        public async Task<ActionResult<APIResponse>> GetAllInstructors()
        {
            var instructors = await _instructorService.GetAllInstructors();
            if (instructors == null || !instructors.Any())
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.ErrorMessages.Add("No instructors found");
                return NotFound(_response);
            }

            var result = instructors.Select(instructor => new
            {
                UserId = instructor.User?.Id,
                UserName = instructor.User?.UserName,
                Email = instructor.User?.Email,
                PhoneNumber = instructor.User?.PhoneNumber,
                InstructorId = instructor.Id,
                CardName = instructor.CardName,
                CardProvider = instructor.CardProvider,
                CardNumber = instructor.CardNumber,
                SubmitCertificate = instructor.SubmitCertificate,
                StatusInstructor = instructor.StatusInsructor
            });

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = result;

            return Ok(_response);
        }

        /// <summary>
        /// Instructor request a payout
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("instructor/payout")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Instructor")]
        public async Task<ActionResult<APIResponse>> CreatePayoutRequest([FromBody] PayoutRequestDTO payoutRequest)
        {
            if (string.IsNullOrEmpty(payoutRequest.InstructorId) || payoutRequest.Amount <= 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Result = "Invalid userId or amount.";
                return BadRequest(_response);
            }

            // Thực hiện yêu cầu rút tiền
            await _walletService.CreatePayout(payoutRequest.InstructorId, payoutRequest.Amount);

            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = "Payout request created successfully";
            return Ok(_response);
        }


    }
}
