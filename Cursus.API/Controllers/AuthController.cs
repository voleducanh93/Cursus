using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.DTO.Category;
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
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;


        public AuthController(IAuthService authService, APIResponse response, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _authService = authService;
            _response = response;
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Login for user
        /// </summary>
        /// <param name="loginRequestDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var loginResult = await _authService.LoginAsync(loginRequestDTO);

            if (loginResult == null)
            {
                var response = new APIResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Please confirm your email before logging in." }
                };

                return Unauthorized(response);
            }
            var successResponse = new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = loginResult
            };

            return Ok(successResponse);

        }
        /// <summary>
        /// RefreshToken
        /// </summary>
        /// <param name="refreshTokenRequest"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO refreshTokenRequest)
        {
            var responseDTO = await _authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);

            return Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = responseDTO
            });
        }
        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="logoutRequest"></param>
        /// <returns></returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO logoutRequest)
        {
            // Gọi phương thức LogoutAsync để thu hồi Refresh Token
            await _authService.LogoutAsync(logoutRequest.RefreshToken);

            // Trả về phản hồi thành công sau khi đã thu hồi Refresh Token
            return Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Logged out successfully"
            });
        }

        /// <summary>
        /// Register for user
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> Register(UserRegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Result = ModelState;
            }
            var result = await _authService.RegisterAsync(dto);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(result);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token = token, username = result.UserName }, Request.Scheme);

            _emailService.SendEmailConfirmation(result.UserName, confirmationLink);

            _response.IsSuccess = true;

            _response.StatusCode = HttpStatusCode.OK;

            _response.Result = result;

            return Ok(_response);

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
        /// Forget Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("forget-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequestDTO model)
        {


            await _authService.ForgetPassword(model.Email);
            return Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = ("Password reset link has been sent to your email.")
            });
        }

        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found");



            var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (resetResult.Succeeded)
            {
                return Ok(new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = ("Password reset successful.")
                });
            }
            else
            {
                return BadRequest("Invalid token or email.");
            }
        }


    }

}
