﻿using Cursus.Common.Helper;
using Cursus.Data.DTO;
using Cursus.Data.Entities;
using Cursus.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cursus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("default")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmailController(IEmailService emailService, APIResponse response, UserManager<ApplicationUser> userManager)
        {
            _emailService = emailService;
            _response = response;
            _userManager = userManager;
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="requestDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public ActionResult<APIResponse> SendEmail([FromBody] EmailRequestDTO requestDTO)
        {
            try
            {
                _emailService.SendEmail(requestDTO);
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch(Exception e)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(e.Message);
                return BadRequest(_response);
            }
        }

    }
}
